using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class CreateValidationSpecCommandHandler : ICommandHandler<CreateValidationSpecCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public CreateValidationSpecCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(CreateValidationSpecCommand command, CancellationToken cancellationToken = default)
    {
        var schema = await _dbContext.DataSchemas
            .FirstOrDefaultAsync(s => s.Id == command.DataSchemaId, cancellationToken);

        if (schema == null)
            throw new InvalidOperationException($"Data schema {command.DataSchemaId} not found");

        var latestVersion = await _dbContext.ValidationSpecs
            .Where(s => s.DataSchemaId == command.DataSchemaId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);

        var version = latestVersion == null ? 1 : latestVersion.Version + 1;

        var entity = new ValidationSpecEntity
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            DataSchemaId = command.DataSchemaId,
            Version = version,
            Status = SchemaStatus.Draft,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ValidationSpecs.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

