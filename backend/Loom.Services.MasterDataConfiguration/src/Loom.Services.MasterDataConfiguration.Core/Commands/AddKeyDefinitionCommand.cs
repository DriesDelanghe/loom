namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddKeyDefinitionCommand(
    Guid TenantId,
    Guid DataSchemaId,
    string Name,
    bool IsPrimary
);

