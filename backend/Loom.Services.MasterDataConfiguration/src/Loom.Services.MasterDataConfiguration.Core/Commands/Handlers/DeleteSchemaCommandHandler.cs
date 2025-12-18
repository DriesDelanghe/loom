using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class DeleteSchemaCommandHandler : ICommandHandler<DeleteSchemaCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public DeleteSchemaCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(DeleteSchemaCommand command, CancellationToken cancellationToken = default)
    {
        // Get all versions of this schema
        var schemas = await _dbContext.DataSchemas
            .Where(s => s.TenantId == command.TenantId && s.Key == command.SchemaKey && s.Role == command.SchemaRole)
            .ToListAsync(cancellationToken);

        if (schemas.Count == 0)
            throw new InvalidOperationException($"Schema with key '{command.SchemaKey}' and role '{command.SchemaRole}' not found");

        var schemaIds = schemas.Select(s => s.Id).ToList();

        // Check if any of these schemas are referenced by other schemas
        var referencingSchemas = await _dbContext.FieldDefinitions
            .Where(f => f.ElementSchemaId.HasValue && schemaIds.Contains(f.ElementSchemaId.Value))
            .Include(f => f.DataSchema)
            .Select(f => f.DataSchema)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (referencingSchemas.Count > 0)
        {
            var referencingKeys = referencingSchemas
                .Select(s => $"{s.Key} (v{s.Version}, {s.Role})")
                .Distinct()
                .ToList();
            throw new InvalidOperationException(
                $"Cannot delete schema '{command.SchemaKey}' ({command.SchemaRole}) because it is referenced by the following schemas: {string.Join(", ", referencingKeys)}"
            );
        }

        // Delete all versions of the schema (cascade will handle related entities)
        _dbContext.DataSchemas.RemoveRange(schemas);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

