using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record DeleteSchemaCommand(
    string SchemaKey,
    SchemaRole SchemaRole,
    Guid TenantId
);

