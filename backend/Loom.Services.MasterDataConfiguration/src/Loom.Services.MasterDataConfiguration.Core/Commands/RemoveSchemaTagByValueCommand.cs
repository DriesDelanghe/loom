namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record RemoveSchemaTagByValueCommand(
    Guid SchemaId,
    string Tag
);

