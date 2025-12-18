using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class AddNodeCommandHandler : ICommandHandler<AddNodeCommand, Guid>
{
    private readonly ConfigurationDbContext _dbContext;

    public AddNodeCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddNodeCommand command, CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.WorkflowVersions
            .FirstOrDefaultAsync(v => v.Id == command.WorkflowVersionId, cancellationToken);

        if (version == null)
            throw new InvalidOperationException($"Workflow version {command.WorkflowVersionId} not found");

        if (version.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Nodes can only be added to draft versions");

        var keyExists = await _dbContext.Nodes
            .AnyAsync(n => n.WorkflowVersionId == command.WorkflowVersionId && n.Key == command.Key, cancellationToken);

        if (keyExists)
            throw new InvalidOperationException($"Node with key '{command.Key}' already exists in this workflow version");

        var node = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = command.WorkflowVersionId,
            Key = command.Key,
            Name = command.Name,
            Type = command.Type,
            ConfigJson = command.Config?.RootElement.GetRawText(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Nodes.Add(node);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return node.Id;
    }
}


