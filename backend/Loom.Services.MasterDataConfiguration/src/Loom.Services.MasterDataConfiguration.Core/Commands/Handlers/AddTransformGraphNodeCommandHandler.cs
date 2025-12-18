using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddTransformGraphNodeCommandHandler : ICommandHandler<AddTransformGraphNodeCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddTransformGraphNodeCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddTransformGraphNodeCommand command, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.TransformationSpecs
            .FirstOrDefaultAsync(s => s.Id == command.TransformationSpecId, cancellationToken);

        if (spec == null)
            throw new InvalidOperationException($"Transformation spec {command.TransformationSpecId} not found");

        if (spec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft transformation specs can be modified. Current status: {spec.Status}");

        if (spec.Mode != Domain.Transformation.TransformationMode.Advanced)
            throw new InvalidOperationException("Can only add graph nodes to Advanced mode transformation specs");

        // Check for duplicate key
        var existingNode = await _dbContext.TransformGraphNodes
            .FirstOrDefaultAsync(n => n.TransformationSpecId == command.TransformationSpecId && n.Key == command.Key, cancellationToken);

        if (existingNode != null)
            throw new InvalidOperationException($"A node with key '{command.Key}' already exists in this transformation spec");

        var entity = new TransformGraphNodeEntity
        {
            Id = Guid.NewGuid(),
            TransformationSpecId = command.TransformationSpecId,
            Key = command.Key,
            NodeType = command.NodeType,
            OutputType = command.OutputType,
            Config = command.Config
        };

        _dbContext.TransformGraphNodes.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

