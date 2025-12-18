using Loom.Services.MasterDataConfiguration.Domain.Schemas;
using Loom.Services.MasterDataConfiguration.Domain.Validation;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class ValidationSpecEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid DataSchemaId { get; set; }
    public int Version { get; set; }
    public SchemaStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<ValidationRuleEntity> Rules { get; set; } = new List<ValidationRuleEntity>();
    public ICollection<ValidationReferenceEntity> References { get; set; } = new List<ValidationReferenceEntity>();

    public ValidationSpec ToDomain()
    {
        return new ValidationSpec
        {
            Id = Id,
            TenantId = TenantId,
            DataSchemaId = DataSchemaId,
            Version = Version,
            Status = Status,
            Description = Description,
            CreatedAt = CreatedAt,
            PublishedAt = PublishedAt
        };
    }

    public static ValidationSpecEntity FromDomain(ValidationSpec spec)
    {
        return new ValidationSpecEntity
        {
            Id = spec.Id,
            TenantId = spec.TenantId,
            DataSchemaId = spec.DataSchemaId,
            Version = spec.Version,
            Status = spec.Status,
            Description = spec.Description,
            CreatedAt = spec.CreatedAt,
            PublishedAt = spec.PublishedAt
        };
    }
}
