namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record PublishDataSchemaCommand(
    Guid DataSchemaId,
    string PublishedBy
);


