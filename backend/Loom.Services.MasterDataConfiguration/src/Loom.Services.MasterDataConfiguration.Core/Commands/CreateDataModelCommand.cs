namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record CreateDataModelCommand(
    Guid TenantId,
    string Key,
    string Name,
    string? Description
);
