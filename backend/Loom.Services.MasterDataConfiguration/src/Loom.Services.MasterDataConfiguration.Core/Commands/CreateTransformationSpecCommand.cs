using Loom.Services.MasterDataConfiguration.Domain.Transformation;

namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record CreateTransformationSpecCommand(
    Guid TenantId,
    Guid SourceSchemaId,
    Guid TargetSchemaId,
    TransformationMode Mode,
    Cardinality Cardinality,
    string? Description
);

