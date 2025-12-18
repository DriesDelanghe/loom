using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class RemoveNodeCommandHandler : ICommandHandler<RemoveNodeCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public RemoveNodeCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveNodeCommand command, CancellationToken cancellationToken = default)
    {
        var node = await _dbContext.Nodes
            .Include(n => n.WorkflowVersion)
            .FirstOrDefaultAsync(n => n.Id == command.NodeId, cancellationToken);

        if (node == null)
            throw new InvalidOperationException($"Node {command.NodeId} not found");

        if (node.WorkflowVersion.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Nodes can only be removed from draft versions");

        var connections = await _dbContext.Connections
            .Where(c => c.WorkflowVersionId == node.WorkflowVersionId && 
                       (c.FromNodeId == command.NodeId || c.ToNodeId == command.NodeId))
            .ToListAsync(cancellationToken);

        _dbContext.Connections.RemoveRange(connections);
        _dbContext.Nodes.Remove(node);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


