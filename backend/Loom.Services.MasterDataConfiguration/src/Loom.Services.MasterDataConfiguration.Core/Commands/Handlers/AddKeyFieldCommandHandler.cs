using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddKeyFieldCommandHandler : ICommandHandler<AddKeyFieldCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddKeyFieldCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddKeyFieldCommand command, CancellationToken cancellationToken = default)
    {
        var keyDefinition = await _dbContext.KeyDefinitions
            .Include(k => k.DataSchema)
            .ThenInclude(s => s.Fields)
            .FirstOrDefaultAsync(k => k.Id == command.KeyDefinitionId, cancellationToken);

        if (keyDefinition == null)
            throw new InvalidOperationException($"Key definition {command.KeyDefinitionId} not found");

        if (keyDefinition.DataSchema.Status != Domain.Schemas.SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft schemas can be modified. Current status: {keyDefinition.DataSchema.Status}");

        // Validate field path exists in schema
        var fieldExists = keyDefinition.DataSchema.Fields.Any(f => f.Path == command.FieldPath);
        if (!fieldExists)
            throw new InvalidOperationException($"Field path '{command.FieldPath}' does not exist in schema {keyDefinition.DataSchemaId}");

        // Check for duplicate field path in same key
        var duplicateField = await _dbContext.KeyFields
            .FirstOrDefaultAsync(f => f.KeyDefinitionId == command.KeyDefinitionId && f.FieldPath == command.FieldPath, cancellationToken);

        if (duplicateField != null)
            throw new InvalidOperationException($"Field path '{command.FieldPath}' is already used in key definition {command.KeyDefinitionId}");

        var existing = await _dbContext.KeyFields
            .FirstOrDefaultAsync(f => f.KeyDefinitionId == command.KeyDefinitionId && f.Order == command.Order, cancellationToken);

        if (existing != null)
            throw new InvalidOperationException($"Key field with order {command.Order} already exists in key definition {command.KeyDefinitionId}");

        var entity = new KeyFieldEntity
        {
            Id = Guid.NewGuid(),
            KeyDefinitionId = command.KeyDefinitionId,
            FieldPath = command.FieldPath,
            Order = command.Order,
            Normalization = command.Normalization
        };

        _dbContext.KeyFields.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

