using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddValidationReferenceCommandHandler : ICommandHandler<AddValidationReferenceCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddValidationReferenceCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddValidationReferenceCommand command, CancellationToken cancellationToken = default)
    {
        var parentSpec = await _dbContext.ValidationSpecs
            .Include(s => s.Rules)
            .FirstOrDefaultAsync(s => s.Id == command.ParentValidationSpecId, cancellationToken);

        if (parentSpec == null)
            throw new InvalidOperationException($"Parent validation spec {command.ParentValidationSpecId} not found");

        if (parentSpec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft validation specs can be modified. Current status: {parentSpec.Status}");

        var childSpec = await _dbContext.ValidationSpecs
            .FirstOrDefaultAsync(s => s.Id == command.ChildValidationSpecId, cancellationToken);

        if (childSpec == null)
            throw new InvalidOperationException($"Child validation spec {command.ChildValidationSpecId} not found");

        if (childSpec.Status != SchemaStatus.Published)
            throw new InvalidOperationException($"Referenced validation spec {command.ChildValidationSpecId} must be Published");

        var existing = await _dbContext.ValidationReferences
            .FirstOrDefaultAsync(r => r.ParentValidationSpecId == command.ParentValidationSpecId
                && r.FieldPath == command.FieldPath
                && r.ChildValidationSpecId == command.ChildValidationSpecId, cancellationToken);

        if (existing != null)
            throw new InvalidOperationException($"Validation reference already exists for field path '{command.FieldPath}'");

        var entity = new ValidationReferenceEntity
        {
            Id = Guid.NewGuid(),
            ParentValidationSpecId = command.ParentValidationSpecId,
            FieldPath = command.FieldPath,
            ChildValidationSpecId = command.ChildValidationSpecId
        };

        _dbContext.ValidationReferences.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}


