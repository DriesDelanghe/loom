using Loom.Services.MasterDataConfiguration.Domain.Schemas;
using Loom.Services.MasterDataConfiguration.Domain.Transformation;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class TransformationSpecEntity
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

    public ICollection<SimpleTransformRuleEntity> SimpleRules { get; set; } = new List<SimpleTransformRuleEntity>();
    public ICollection<TransformGraphNodeEntity> GraphNodes { get; set; } = new List<TransformGraphNodeEntity>();
    public ICollection<TransformGraphEdgeEntity> GraphEdges { get; set; } = new List<TransformGraphEdgeEntity>();
    public ICollection<TransformOutputBindingEntity> OutputBindings { get; set; } = new List<TransformOutputBindingEntity>();
    public ICollection<TransformReferenceEntity> References { get; set; } = new List<TransformReferenceEntity>();

    public TransformationSpec ToDomain()
    {
        return new TransformationSpec
        {
            Id = Id,
            TenantId = TenantId,
            SourceSchemaId = SourceSchemaId,
            TargetSchemaId = TargetSchemaId,
            Mode = Mode,
            Cardinality = Cardinality,
            Version = Version,
            Status = Status,
            Description = Description,
            CreatedAt = CreatedAt,
            PublishedAt = PublishedAt
        };
    }

    public static TransformationSpecEntity FromDomain(TransformationSpec spec)
    {
        return new TransformationSpecEntity
        {
            Id = spec.Id,
            TenantId = spec.TenantId,
            SourceSchemaId = spec.SourceSchemaId,
            TargetSchemaId = spec.TargetSchemaId,
            Mode = spec.Mode,
            Cardinality = spec.Cardinality,
            Version = spec.Version,
            Status = spec.Status,
            Description = spec.Description,
            CreatedAt = spec.CreatedAt,
            PublishedAt = spec.PublishedAt
        };
    }
}
