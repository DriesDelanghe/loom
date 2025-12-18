using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Domain.Transformation;

public class TransformationSpec
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SourceSchemaId { get; set; }
    public Guid TargetSchemaId { get; set; }
    public TransformationMode Mode { get; set; }
    public Cardinality Cardinality { get; set; }
    public int Version { get; set; }
    public SchemaStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}


