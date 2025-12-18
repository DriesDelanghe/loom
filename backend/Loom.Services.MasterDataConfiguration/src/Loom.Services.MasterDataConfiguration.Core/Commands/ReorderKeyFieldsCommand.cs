namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record ReorderKeyFieldsCommand(
    Guid KeyDefinitionId,
    IReadOnlyList<Guid> KeyFieldIdsInOrder
);


