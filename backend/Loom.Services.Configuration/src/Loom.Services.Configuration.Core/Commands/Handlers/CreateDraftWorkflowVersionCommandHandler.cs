using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class CreateDraftWorkflowVersionCommandHandler : ICommandHandler<CreateDraftWorkflowVersionCommand, Guid>
{
    private readonly ConfigurationDbContext _dbContext;

    public CreateDraftWorkflowVersionCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(CreateDraftWorkflowVersionCommand command, CancellationToken cancellationToken = default)
    {
        var publishedVersion = await _dbContext.WorkflowVersions
            .Include(v => v.Nodes)
            .Include(v => v.Connections)
            .Include(v => v.Variables)
            .Include(v => v.Labels)
            .Include(v => v.Settings)
            .Include(v => v.TriggerBindings)
                .ThenInclude(tb => tb.NodeBindings)
            .Where(v => v.DefinitionId == command.WorkflowDefinitionId && v.Status == WorkflowStatus.Published)
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync(cancellationToken);

        var nextVersion = publishedVersion != null
            ? publishedVersion.Version + 1
            : (await _dbContext.WorkflowVersions
                .Where(v => v.DefinitionId == command.WorkflowDefinitionId)
                .Select(v => (int?)v.Version)
                .MaxAsync(cancellationToken) ?? 0) + 1;

        var newVersion = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = command.WorkflowDefinitionId,
            Version = nextVersion,
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = command.CreatedBy
        };

        if (publishedVersion != null)
        {
            foreach (var node in publishedVersion.Nodes)
            {
                newVersion.Nodes.Add(new NodeEntity
                {
                    Id = Guid.NewGuid(),
                    WorkflowVersionId = newVersion.Id,
                    Key = node.Key,
                    Name = node.Name,
                    Type = node.Type,
                    ConfigJson = node.ConfigJson,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var nodeMapping = newVersion.Nodes
                .Join(publishedVersion.Nodes, 
                    newNode => newNode.Key, 
                    oldNode => oldNode.Key,
                    (newNode, oldNode) => new { OldId = oldNode.Id, NewId = newNode.Id })
                .ToDictionary(x => x.OldId, x => x.NewId);

            foreach (var connection in publishedVersion.Connections)
            {
                if (nodeMapping.TryGetValue(connection.FromNodeId, out var newFromId) &&
                    nodeMapping.TryGetValue(connection.ToNodeId, out var newToId))
                {
                    newVersion.Connections.Add(new ConnectionEntity
                    {
                        Id = Guid.NewGuid(),
                        WorkflowVersionId = newVersion.Id,
                        FromNodeId = newFromId,
                        ToNodeId = newToId,
                        Outcome = connection.Outcome,
                        Order = connection.Order
                    });
                }
            }

            foreach (var variable in publishedVersion.Variables)
            {
                newVersion.Variables.Add(new WorkflowVariableEntity
                {
                    Id = Guid.NewGuid(),
                    WorkflowVersionId = newVersion.Id,
                    Key = variable.Key,
                    Type = variable.Type,
                    InitialValueJson = variable.InitialValueJson,
                    Description = variable.Description
                });
            }

            foreach (var label in publishedVersion.Labels)
            {
                newVersion.Labels.Add(new WorkflowLabelDefinitionEntity
                {
                    Id = Guid.NewGuid(),
                    WorkflowVersionId = newVersion.Id,
                    Key = label.Key,
                    Type = label.Type,
                    Description = label.Description
                });
            }

            if (publishedVersion.Settings != null)
            {
                newVersion.Settings = new WorkflowSettingsEntity
                {
                    Id = Guid.NewGuid(),
                    WorkflowVersionId = newVersion.Id,
                    MaxNodeExecutions = publishedVersion.Settings.MaxNodeExecutions,
                    MaxDurationSeconds = publishedVersion.Settings.MaxDurationSeconds
                };
            }

            foreach (var triggerBinding in publishedVersion.TriggerBindings)
            {
                var newTriggerBinding = new TriggerBindingEntity
                {
                    Id = Guid.NewGuid(),
                    TriggerId = triggerBinding.TriggerId,
                    WorkflowVersionId = newVersion.Id,
                    Enabled = triggerBinding.Enabled,
                    Priority = triggerBinding.Priority
                };

                foreach (var nodeBinding in triggerBinding.NodeBindings)
                {
                    if (nodeMapping.TryGetValue(nodeBinding.EntryNodeId, out var newEntryNodeId))
                    {
                        newTriggerBinding.NodeBindings.Add(new TriggerNodeBindingEntity
                        {
                            Id = Guid.NewGuid(),
                            TriggerBindingId = newTriggerBinding.Id,
                            EntryNodeId = newEntryNodeId,
                            Order = nodeBinding.Order
                        });
                    }
                }

                _dbContext.TriggerBindings.Add(newTriggerBinding);
            }
        }

        _dbContext.WorkflowVersions.Add(newVersion);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newVersion.Id;
    }
}

