namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record CreateValidationSpecCommand(
    Guid TenantId,
    Guid DataSchemaId,
    string? Description
);


