namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddKeyFieldCommand(
    Guid KeyDefinitionId,
    string FieldPath,
    int Order,
    string? Normalization
);

