using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Queries;

public record GetSchemasQuery(
    Guid TenantId,
    SchemaRole? Role,
    SchemaStatus? Status
);


