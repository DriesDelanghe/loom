using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class CreateDataSchemaCommandHandler : ICommandHandler<CreateDataSchemaCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public CreateDataSchemaCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(CreateDataSchemaCommand command, CancellationToken cancellationToken = default)
    {
        // DataModelId is optional when creating - it will be validated when publishing
        if (command.DataModelId.HasValue)
        {
            var model = await _dbContext.DataModels
                .FirstOrDefaultAsync(m => m.Id == command.DataModelId.Value, cancellationToken);

            if (model == null)
                throw new InvalidOperationException($"Data model {command.DataModelId.Value} not found");
        }

        // Check for existing schema with same key and role
        var existingSchema = await _dbContext.DataSchemas
            .Where(s => s.TenantId == command.TenantId && s.Key == command.Key && s.Role == command.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingSchema != null)
            throw new InvalidOperationException($"Schema with key '{command.Key}' and role '{command.Role}' already exists");

        // Get latest version for this key and role combination
        var latestVersion = await _dbContext.DataSchemas
            .Where(s => s.TenantId == command.TenantId && s.Key == command.Key && s.Role == command.Role)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);

        var version = latestVersion == null ? 1 : latestVersion.Version + 1;

        var entity = new DataSchemaEntity
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            DataModelId = command.DataModelId,
            Role = command.Role,
            Key = command.Key,
            Version = version,
            Status = SchemaStatus.Draft,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.DataSchemas.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

