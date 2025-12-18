using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class UpdateNodeMetadataCommandHandler : ICommandHandler<UpdateNodeMetadataCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public UpdateNodeMetadataCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UpdateNodeMetadataCommand command, CancellationToken cancellationToken = default)
    {
        var node = await _dbContext.Nodes
            .Include(n => n.WorkflowVersion)
            .FirstOrDefaultAsync(n => n.Id == command.NodeId, cancellationToken);

        if (node == null)
            throw new InvalidOperationException($"Node {command.NodeId} not found");

        if (node.WorkflowVersion.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Node metadata can only be updated in draft versions");

        node.Name = command.Name;
        
        if (command.Type.HasValue)
        {
            node.Type = command.Type.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


