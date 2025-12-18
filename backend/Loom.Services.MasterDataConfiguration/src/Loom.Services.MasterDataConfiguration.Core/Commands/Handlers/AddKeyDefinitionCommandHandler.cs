using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddKeyDefinitionCommandHandler : ICommandHandler<AddKeyDefinitionCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddKeyDefinitionCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddKeyDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var schema = await _dbContext.DataSchemas
            .FirstOrDefaultAsync(s => s.Id == command.DataSchemaId, cancellationToken);

        if (schema == null)
            throw new InvalidOperationException($"Data schema {command.DataSchemaId} not found");

        if (schema.Role != SchemaRole.Master)
            throw new InvalidOperationException("Key definitions can only be added to Master schemas");

        if (schema.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft schemas can be modified. Current status: {schema.Status}");

        var existing = await _dbContext.KeyDefinitions
            .FirstOrDefaultAsync(k => k.DataSchemaId == command.DataSchemaId && k.Name == command.Name, cancellationToken);

        if (existing != null)
            throw new InvalidOperationException($"Key definition with name '{command.Name}' already exists in schema {command.DataSchemaId}");

        // Check if there's already a primary key
        if (command.IsPrimary)
        {
            var existingPrimary = await _dbContext.KeyDefinitions
                .FirstOrDefaultAsync(k => k.DataSchemaId == command.DataSchemaId && k.IsPrimary, cancellationToken);

            if (existingPrimary != null)
                throw new InvalidOperationException($"Schema {command.DataSchemaId} already has a primary key definition");
        }

        var entity = new KeyDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            DataSchemaId = command.DataSchemaId,
            Name = command.Name,
            IsPrimary = command.IsPrimary,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.KeyDefinitions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

