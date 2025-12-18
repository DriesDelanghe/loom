namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddSchemaTagCommand(
    Guid SchemaId,
    string Tag
);

