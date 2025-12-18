using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Core.Services;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class PublishDataSchemaCommandHandler : ICommandHandler<PublishDataSchemaCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;
    private readonly IStaticValidationEngine _validationEngine;

    public PublishDataSchemaCommandHandler(
        MasterDataConfigurationDbContext dbContext,
        IStaticValidationEngine validationEngine)
    {
        _dbContext = dbContext;
        _validationEngine = validationEngine;
    }

    public async Task<bool> HandleAsync(PublishDataSchemaCommand command, CancellationToken cancellationToken = default)
    {
        var schema = await _dbContext.DataSchemas
            .FirstOrDefaultAsync(s => s.Id == command.DataSchemaId, cancellationToken);

        if (schema == null)
            throw new InvalidOperationException($"Data schema {command.DataSchemaId} not found");

        if (schema.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft schemas can be published. Current status: {schema.Status}");

        // Validate with strict rules for publishing (requires Published references)
        var validationResult = await _validationEngine.ValidateSchemaAsync(command.DataSchemaId, forPublish: true, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessages = string.Join(", ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
            throw new InvalidOperationException($"Schema validation failed: {errorMessages}");
        }

        var previousPublished = await _dbContext.DataSchemas
            .Where(s => s.TenantId == schema.TenantId && s.Key == schema.Key && s.Status == SchemaStatus.Published)
            .ToListAsync(cancellationToken);

        foreach (var prev in previousPublished)
        {
            prev.Status = SchemaStatus.Archived;
        }

        schema.Status = SchemaStatus.Published;
        schema.PublishedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

