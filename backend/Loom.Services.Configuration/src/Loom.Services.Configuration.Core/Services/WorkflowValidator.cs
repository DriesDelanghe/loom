using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Services;

public class WorkflowValidator : IWorkflowValidator
{
    private readonly ConfigurationDbContext _dbContext;

    public WorkflowValidator(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ValidationResult> ValidateAsync(Guid workflowVersionId, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        var version = await _dbContext.WorkflowVersions
            .Include(v => v.Nodes)
            .Include(v => v.Connections)
            .Include(v => v.Variables)
            .Include(v => v.Labels)
            .Include(v => v.TriggerBindings)
                .ThenInclude(tb => tb.NodeBindings)
            .FirstOrDefaultAsync(v => v.Id == workflowVersionId, cancellationToken);

        if (version == null)
        {
            errors.Add($"Workflow version {workflowVersionId} not found");
            return new ValidationResult(false, errors, warnings);
        }

        var nodeIds = version.Nodes.Select(n => n.Id).ToHashSet();
        var nodeKeys = version.Nodes.Select(n => n.Key).ToList();

        if (nodeKeys.Count != nodeKeys.Distinct().Count())
        {
            errors.Add("Duplicate node keys found");
        }

        foreach (var connection in version.Connections)
        {
            if (!nodeIds.Contains(connection.FromNodeId))
                errors.Add($"Connection references non-existent from node: {connection.FromNodeId}");

            if (!nodeIds.Contains(connection.ToNodeId))
                errors.Add($"Connection references non-existent to node: {connection.ToNodeId}");
        }

        var variableKeys = version.Variables.Select(v => v.Key).ToList();
        if (variableKeys.Count != variableKeys.Distinct().Count())
        {
            errors.Add("Duplicate variable keys found");
        }

        var labelKeys = version.Labels.Select(l => l.Key).ToList();
        if (labelKeys.Count != labelKeys.Distinct().Count())
        {
            errors.Add("Duplicate label keys found");
        }

        // Validate node configs
        foreach (var node in version.Nodes)
        {
            NodeConfigValidator.ValidateConfig(node, errors);
        }

        // Validate semantic outcomes
        ValidateOutcomes(version.Nodes.ToList(), version.Connections.ToList(), errors);

        // Validate control nodes
        ValidateControlNodes(version.Nodes.ToList(), version.Connections.ToList(), errors);

        // Validate implicit workflow ends
        ValidateWorkflowEnds(version.Nodes.ToList(), version.Connections.ToList(), version.TriggerBindings.SelectMany(tb => tb.NodeBindings).Select(nb => nb.EntryNodeId).ToHashSet(), errors);

        var connectionsList = version.Connections.ToList();
        var nodesWithConnections = connectionsList
            .SelectMany(c => new[] { c.FromNodeId, c.ToNodeId })
            .ToHashSet();

        var isolatedNodes = nodeIds
            .Where(id => !nodesWithConnections.Contains(id))
            .ToList();

        var reachableNodes = GetReachableNodes(version.Nodes.ToList(), connectionsList);
        var unreachableNodes = nodeIds
            .Except(reachableNodes)
            .Except(isolatedNodes)
            .ToList();

        if (isolatedNodes.Any())
        {
            warnings.Add($"Found {isolatedNodes.Count} isolated node(s) with no connections");
        }

        if (unreachableNodes.Any())
        {
            warnings.Add($"Found {unreachableNodes.Count} unreachable node(s)");
        }

        var cycles = DetectCycles(version.Nodes.ToList(), version.Connections.ToList());
        if (cycles.Any())
        {
            warnings.Add($"Found {cycles.Count} potential cycle(s) in the graph");
        }

        return new ValidationResult(errors.Count == 0, errors, warnings);
    }

    private void ValidateOutcomes(List<NodeEntity> nodes, List<ConnectionEntity> connections, List<string> errors)
    {
        var nodeDict = nodes.ToDictionary(n => n.Id);

        foreach (var node in nodes)
        {
            var nodeType = node.Type;
            var allowedOutcomes = NodeTypeMetadata.GetAllowedOutcomes(nodeType);
            var category = NodeTypeMetadata.GetCategory(nodeType);

            var outgoingConnections = connections.Where(c => c.FromNodeId == node.Id).ToList();

            // Check that all outcomes are allowed
            foreach (var conn in outgoingConnections)
            {
                if (!allowedOutcomes.Contains(conn.Outcome))
                {
                    errors.Add($"Node '{node.Key}' (type: {nodeType}) has connection with invalid outcome '{conn.Outcome}'. Allowed outcomes: {string.Join(", ", allowedOutcomes)}");
                }
            }

            // Check for duplicate semantic outcomes (for non-Control nodes)
            if (!NodeTypeMetadata.IsControlNode(nodeType))
            {
                var outcomeGroups = outgoingConnections.GroupBy(c => c.Outcome).ToList();
                foreach (var group in outcomeGroups)
                {
                    if (group.Count() > 1)
                    {
                        errors.Add($"Node '{node.Key}' (type: {nodeType}) has {group.Count()} connections with the same outcome '{group.Key}'. Each semantic outcome can only be used once.");
                    }
                }
            }

            // Check required outcomes for non-Control nodes
            if (!NodeTypeMetadata.IsControlNode(nodeType))
            {
                var requiredOutcomes = NodeTypeMetadata.GetRequiredOutcomes(nodeType);
                var presentOutcomes = outgoingConnections.Select(c => c.Outcome).ToHashSet();

                foreach (var required in requiredOutcomes)
                {
                    if (!presentOutcomes.Contains(required))
                    {
                        errors.Add($"Node '{node.Key}' (type: {nodeType}) is missing required outcome '{required}'");
                    }
                }
            }
        }
    }

    private void ValidateControlNodes(List<NodeEntity> nodes, List<ConnectionEntity> connections, List<string> errors)
    {
        var nodeDict = nodes.ToDictionary(n => n.Id);

        foreach (var node in nodes)
        {
            if (!NodeTypeMetadata.IsControlNode(node.Type))
                continue;

            var incoming = connections.Count(c => c.ToNodeId == node.Id);
            var outgoing = connections.Where(c => c.FromNodeId == node.Id).ToList();

            if (node.Type == NodeType.Split)
            {
                if (incoming != 1)
                {
                    errors.Add($"Split node '{node.Key}' must have exactly 1 incoming connection, but has {incoming}");
                }

                if (outgoing.Count < 2)
                {
                    errors.Add($"Split node '{node.Key}' must have at least 2 outgoing connections, but has {outgoing.Count}");
                }

                foreach (var conn in outgoing)
                {
                    if (conn.Outcome != "Next")
                    {
                        errors.Add($"Split node '{node.Key}' outgoing connection must have outcome 'Next', but has '{conn.Outcome}'");
                    }
                }
            }
            else if (node.Type == NodeType.Join)
            {
                if (incoming < 2)
                {
                    errors.Add($"Join node '{node.Key}' must have at least 2 incoming connections, but has {incoming}");
                }

                if (outgoing.Count != 1)
                {
                    errors.Add($"Join node '{node.Key}' must have exactly 1 outgoing connection, but has {outgoing.Count}");
                }

                if (outgoing.Count == 1 && outgoing[0].Outcome != "Joined")
                {
                    errors.Add($"Join node '{node.Key}' outgoing connection must have outcome 'Joined', but has '{outgoing[0].Outcome}'");
                }
            }
        }
    }

    private void ValidateWorkflowEnds(List<NodeEntity> nodes, List<ConnectionEntity> connections, HashSet<Guid> entryNodeIds, List<string> errors)
    {
        var nodesWithOutgoing = connections.Select(c => c.FromNodeId).ToHashSet();
        var endNodes = nodes.Where(n => !nodesWithOutgoing.Contains(n.Id)).ToList();

        if (endNodes.Count == 0)
        {
            errors.Add("Workflow must have at least one end node (node with no outgoing connections)");
            return;
        }

        foreach (var endNode in endNodes)
        {
            if (NodeTypeMetadata.IsControlNode(endNode.Type))
            {
                errors.Add($"End node '{endNode.Key}' cannot be a Control node (type: {endNode.Type})");
            }

            if (entryNodeIds.Contains(endNode.Id))
            {
                errors.Add($"End node '{endNode.Key}' cannot be a trigger entry node");
            }
        }
    }

    private HashSet<Guid> GetReachableNodes(List<NodeEntity> nodes, List<ConnectionEntity> connections)
    {
        var reachable = new HashSet<Guid>();
        var visited = new HashSet<Guid>();
        
        var nodesWithIncoming = connections
            .Select(c => c.ToNodeId)
            .ToHashSet();

        var nodesWithOutgoing = connections
            .Select(c => c.FromNodeId)
            .ToHashSet();

        var entryNodes = nodes
            .Where(n => !nodesWithIncoming.Contains(n.Id))
            .Select(n => n.Id)
            .ToList();

        void Dfs(Guid nodeId)
        {
            if (visited.Contains(nodeId))
                return;

            visited.Add(nodeId);
            reachable.Add(nodeId);

            var outgoing = connections
                .Where(c => c.FromNodeId == nodeId)
                .Select(c => c.ToNodeId);

            foreach (var target in outgoing)
            {
                Dfs(target);
            }
        }

        foreach (var entryNodeId in entryNodes)
        {
            Dfs(entryNodeId);
        }

        if (entryNodes.Count == 0 && nodes.Count > 0)
        {
            foreach (var node in nodes)
            {
                if (!visited.Contains(node.Id))
                {
                    Dfs(node.Id);
                }
            }
        }

        return reachable;
    }

    private List<List<Guid>> DetectCycles(List<NodeEntity> nodes, List<ConnectionEntity> connections)
    {
        var cycles = new List<List<Guid>>();
        var visited = new HashSet<Guid>();
        var recStack = new HashSet<Guid>();
        var path = new List<Guid>();

        void Dfs(Guid nodeId)
        {
            if (recStack.Contains(nodeId))
            {
                var cycleStart = path.IndexOf(nodeId);
                if (cycleStart >= 0)
                {
                    cycles.Add(path.Skip(cycleStart).Append(nodeId).ToList());
                }
                return;
            }

            if (visited.Contains(nodeId))
                return;

            visited.Add(nodeId);
            recStack.Add(nodeId);
            path.Add(nodeId);

            var outgoing = connections
                .Where(c => c.FromNodeId == nodeId)
                .Select(c => c.ToNodeId);

            foreach (var target in outgoing)
            {
                Dfs(target);
            }

            recStack.Remove(nodeId);
            path.RemoveAt(path.Count - 1);
        }

        foreach (var node in nodes)
        {
            if (!visited.Contains(node.Id))
            {
                Dfs(node.Id);
            }
        }

        return cycles;
    }
}


