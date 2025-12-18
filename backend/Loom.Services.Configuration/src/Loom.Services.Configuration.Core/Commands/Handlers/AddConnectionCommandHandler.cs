using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class AddConnectionCommandHandler : ICommandHandler<AddConnectionCommand, Guid>
{
    private readonly ConfigurationDbContext _dbContext;

    public AddConnectionCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddConnectionCommand command, CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.WorkflowVersions
            .FirstOrDefaultAsync(v => v.Id == command.WorkflowVersionId, cancellationToken);

        if (version == null)
            throw new InvalidOperationException($"Workflow version {command.WorkflowVersionId} not found");

        if (version.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Connections can only be added to draft versions");

        var fromNodeExists = await _dbContext.Nodes
            .AnyAsync(n => n.Id == command.FromNodeId && n.WorkflowVersionId == command.WorkflowVersionId, cancellationToken);

        var toNodeExists = await _dbContext.Nodes
            .AnyAsync(n => n.Id == command.ToNodeId && n.WorkflowVersionId == command.WorkflowVersionId, cancellationToken);

        if (!fromNodeExists || !toNodeExists)
            throw new InvalidOperationException("Both nodes must exist in the workflow version");

        var fromNode = await _dbContext.Nodes
            .FirstOrDefaultAsync(n => n.Id == command.FromNodeId && n.WorkflowVersionId == command.WorkflowVersionId, cancellationToken);

        if (fromNode == null)
            throw new InvalidOperationException("From node not found");

        var existingConnection = await _dbContext.Connections
            .AnyAsync(c => c.WorkflowVersionId == command.WorkflowVersionId &&
                          c.FromNodeId == command.FromNodeId &&
                          c.ToNodeId == command.ToNodeId &&
                          c.Outcome == command.Outcome, cancellationToken);

        if (existingConnection)
            throw new InvalidOperationException($"A connection with outcome '{command.Outcome}' already exists between these nodes");

        var connection = new ConnectionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = command.WorkflowVersionId,
            FromNodeId = command.FromNodeId,
            ToNodeId = command.ToNodeId,
            Outcome = command.Outcome,
            Order = command.Order
        };

        _dbContext.Connections.Add(connection);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return connection.Id;
    }
}


