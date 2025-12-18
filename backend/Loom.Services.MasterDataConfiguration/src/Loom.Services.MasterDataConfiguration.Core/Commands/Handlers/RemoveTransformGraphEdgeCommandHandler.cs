using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class RemoveTransformGraphEdgeCommandHandler : ICommandHandler<RemoveTransformGraphEdgeCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public RemoveTransformGraphEdgeCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveTransformGraphEdgeCommand command, CancellationToken cancellationToken = default)
    {
        var edge = await _dbContext.TransformGraphEdges
            .Include(e => e.TransformationSpec)
            .FirstOrDefaultAsync(e => e.Id == command.EdgeId, cancellationToken);

        if (edge == null)
            throw new InvalidOperationException($"Graph edge {command.EdgeId} not found");

        if (edge.TransformationSpec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft transformation specs can be modified. Current status: {edge.TransformationSpec.Status}");

        _dbContext.TransformGraphEdges.Remove(edge);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

