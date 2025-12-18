using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Core.Services;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class PublishRelatedSchemasCommandHandler : ICommandHandler<PublishRelatedSchemasCommand, IReadOnlyList<Guid>>
{
    private readonly MasterDataConfigurationDbContext _dbContext;
    private readonly IStaticValidationEngine _validationEngine;

    public PublishRelatedSchemasCommandHandler(
        MasterDataConfigurationDbContext dbContext,
        IStaticValidationEngine validationEngine)
    {
        _dbContext = dbContext;
        _validationEngine = validationEngine;
    }

    public async Task<IReadOnlyList<Guid>> HandleAsync(PublishRelatedSchemasCommand command, CancellationToken cancellationToken = default)
    {
        var publishedSchemaIds = new List<Guid>();

        foreach (var relatedSchemaId in command.RelatedSchemaIds)
        {
            var schema = await _dbContext.DataSchemas
                .FirstOrDefaultAsync(s => s.Id == relatedSchemaId, cancellationToken);

            if (schema == null)
                throw new InvalidOperationException($"Schema {relatedSchemaId} not found");

            if (schema.Status != SchemaStatus.Draft)
            {
                // Skip if already published or archived
                continue;
            }

            // Validate with strict rules for publishing
            var validationResult = await _validationEngine.ValidateSchemaAsync(relatedSchemaId, forPublish: true, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = string.Join(", ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
                throw new InvalidOperationException($"Schema '{schema.Key}' (v{schema.Version}) validation failed: {errorMessages}");
            }

            // Archive previous published versions
            var previousPublished = await _dbContext.DataSchemas
                .Where(s => s.TenantId == schema.TenantId && s.Key == schema.Key && s.Status == SchemaStatus.Published)
                .ToListAsync(cancellationToken);

            foreach (var prev in previousPublished)
            {
                prev.Status = SchemaStatus.Archived;
            }

            // Publish the schema
            schema.Status = SchemaStatus.Published;
            schema.PublishedAt = DateTime.UtcNow;

            publishedSchemaIds.Add(schema.Id);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return publishedSchemaIds;
    }
}

