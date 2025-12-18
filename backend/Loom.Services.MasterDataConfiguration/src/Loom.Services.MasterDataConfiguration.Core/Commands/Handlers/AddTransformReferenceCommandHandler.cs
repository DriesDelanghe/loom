using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddTransformReferenceCommandHandler : ICommandHandler<AddTransformReferenceCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddTransformReferenceCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddTransformReferenceCommand command, CancellationToken cancellationToken = default)
    {
        var parentSpec = await _dbContext.TransformationSpecs
            .FirstOrDefaultAsync(s => s.Id == command.ParentTransformationSpecId, cancellationToken);

        if (parentSpec == null)
            throw new InvalidOperationException($"Parent transformation spec {command.ParentTransformationSpecId} not found");

        if (parentSpec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft transformation specs can be modified. Current status: {parentSpec.Status}");

        var childSpec = await _dbContext.TransformationSpecs
            .FirstOrDefaultAsync(s => s.Id == command.ChildTransformationSpecId, cancellationToken);

        if (childSpec == null)
            throw new InvalidOperationException($"Child transformation spec {command.ChildTransformationSpecId} not found");

        if (childSpec.Status != SchemaStatus.Published)
            throw new InvalidOperationException($"Referenced transformation spec {command.ChildTransformationSpecId} must be Published");

        var existing = await _dbContext.TransformReferences
            .FirstOrDefaultAsync(r => r.ParentTransformationSpecId == command.ParentTransformationSpecId
                && r.SourceFieldPath == command.SourceFieldPath
                && r.TargetFieldPath == command.TargetFieldPath, cancellationToken);

        if (existing != null)
            throw new InvalidOperationException($"Transform reference already exists for source path '{command.SourceFieldPath}' and target path '{command.TargetFieldPath}'");

        var entity = new TransformReferenceEntity
        {
            Id = Guid.NewGuid(),
            ParentTransformationSpecId = command.ParentTransformationSpecId,
            SourceFieldPath = command.SourceFieldPath,
            TargetFieldPath = command.TargetFieldPath,
            ChildTransformationSpecId = command.ChildTransformationSpecId
        };

        _dbContext.TransformReferences.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}


