using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddTransformOutputBindingCommandHandler : ICommandHandler<AddTransformOutputBindingCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddTransformOutputBindingCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddTransformOutputBindingCommand command, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.TransformationSpecs
            .Include(s => s.GraphNodes)
            .FirstOrDefaultAsync(s => s.Id == command.TransformationSpecId, cancellationToken);

        if (spec == null)
            throw new InvalidOperationException($"Transformation spec {command.TransformationSpecId} not found");

        if (spec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft transformation specs can be modified. Current status: {spec.Status}");

        if (spec.Mode != Domain.Transformation.TransformationMode.Advanced)
            throw new InvalidOperationException("Can only add output bindings to Advanced mode transformation specs");

        // Verify node exists and belongs to this spec
        var node = spec.GraphNodes.FirstOrDefault(n => n.Id == command.FromNodeId);
        if (node == null)
            throw new InvalidOperationException($"Node {command.FromNodeId} not found");

        // Check for duplicate target path
        var existingBinding = await _dbContext.TransformOutputBindings
            .FirstOrDefaultAsync(b => b.TransformationSpecId == command.TransformationSpecId && b.TargetPath == command.TargetPath, cancellationToken);

        if (existingBinding != null)
            throw new InvalidOperationException($"An output binding for target path '{command.TargetPath}' already exists");

        var entity = new TransformOutputBindingEntity
        {
            Id = Guid.NewGuid(),
            TransformationSpecId = command.TransformationSpecId,
            TargetPath = command.TargetPath,
            FromNodeId = command.FromNodeId
        };

        _dbContext.TransformOutputBindings.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}


