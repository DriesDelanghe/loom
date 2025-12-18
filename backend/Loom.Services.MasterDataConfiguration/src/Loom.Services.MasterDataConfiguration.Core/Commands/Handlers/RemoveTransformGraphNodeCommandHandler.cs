using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class RemoveTransformGraphNodeCommandHandler : ICommandHandler<RemoveTransformGraphNodeCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public RemoveTransformGraphNodeCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveTransformGraphNodeCommand command, CancellationToken cancellationToken = default)
    {
        var node = await _dbContext.TransformGraphNodes
            .Include(n => n.TransformationSpec)
            .FirstOrDefaultAsync(n => n.Id == command.NodeId, cancellationToken);

        if (node == null)
            throw new InvalidOperationException($"Graph node {command.NodeId} not found");

        if (node.TransformationSpec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft transformation specs can be modified. Current status: {node.TransformationSpec.Status}");

        // Edges and output bindings will be cascade deleted
        _dbContext.TransformGraphNodes.Remove(node);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


