using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddTransformGraphEdgeCommandHandler : ICommandHandler<AddTransformGraphEdgeCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddTransformGraphEdgeCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddTransformGraphEdgeCommand command, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.TransformationSpecs
            .Include(s => s.GraphNodes)
            .FirstOrDefaultAsync(s => s.Id == command.TransformationSpecId, cancellationToken);

        if (spec == null)
            throw new InvalidOperationException($"Transformation spec {command.TransformationSpecId} not found");

        if (spec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft transformation specs can be modified. Current status: {spec.Status}");

        if (spec.Mode != Domain.Transformation.TransformationMode.Advanced)
            throw new InvalidOperationException("Can only add graph edges to Advanced mode transformation specs");

        // Verify nodes exist and belong to this spec
        var fromNode = spec.GraphNodes.FirstOrDefault(n => n.Id == command.FromNodeId);
        var toNode = spec.GraphNodes.FirstOrDefault(n => n.Id == command.ToNodeId);

        if (fromNode == null)
            throw new InvalidOperationException($"Source node {command.FromNodeId} not found");

        if (toNode == null)
            throw new InvalidOperationException($"Target node {command.ToNodeId} not found");

        var entity = new TransformGraphEdgeEntity
        {
            Id = Guid.NewGuid(),
            TransformationSpecId = command.TransformationSpecId,
            FromNodeId = command.FromNodeId,
            ToNodeId = command.ToNodeId,
            InputName = command.InputName,
            Order = command.Order
        };

        _dbContext.TransformGraphEdges.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

