namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record PublishRelatedSchemasCommand(
    Guid SchemaId,
    string PublishedBy,
    IReadOnlyList<Guid> RelatedSchemaIds
);

