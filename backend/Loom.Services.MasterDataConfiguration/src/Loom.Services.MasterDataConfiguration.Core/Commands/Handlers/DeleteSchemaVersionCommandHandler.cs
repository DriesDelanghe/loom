using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class DeleteSchemaVersionCommandHandler : ICommandHandler<DeleteSchemaVersionCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public DeleteSchemaVersionCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(DeleteSchemaVersionCommand command, CancellationToken cancellationToken = default)
    {
        var schema = await _dbContext.DataSchemas
            .FirstOrDefaultAsync(s => s.Id == command.SchemaVersionId, cancellationToken);

        if (schema == null)
            throw new InvalidOperationException($"Schema version {command.SchemaVersionId} not found");

        // Check if this is the latest version
        var latestVersion = await _dbContext.DataSchemas
            .Where(s => s.TenantId == schema.TenantId && s.Key == schema.Key && s.Role == schema.Role)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestVersion == null || latestVersion.Id != schema.Id)
            throw new InvalidOperationException("Only the latest version of a schema can be deleted");

        // Delete the schema (cascade will handle related entities)
        _dbContext.DataSchemas.Remove(schema);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

