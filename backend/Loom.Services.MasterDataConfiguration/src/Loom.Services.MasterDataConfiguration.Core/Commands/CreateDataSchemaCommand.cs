using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record CreateDataSchemaCommand(
    Guid TenantId,
    Guid? DataModelId,
    SchemaRole Role,
    string Key,
    string? Description
);


