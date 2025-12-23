namespace Loom.Services.MasterDataConfiguration.Core.Queries;

public record GetCompatibleTransformationSpecsQuery(
    Guid SourceSchemaId,
    Guid TargetSchemaId,
    Domain.Schemas.SchemaStatus? Status = null
);

