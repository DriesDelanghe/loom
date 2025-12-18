using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class DataSchemaEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? DataModelId { get; set; }
    public SchemaRole Role { get; set; }
    public string Key { get; set; } = default!;
    public int Version { get; set; }
    public SchemaStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<FieldDefinitionEntity> Fields { get; set; } = new List<FieldDefinitionEntity>();
    public ICollection<SchemaTagEntity> Tags { get; set; } = new List<SchemaTagEntity>();
    public ICollection<KeyDefinitionEntity> KeyDefinitions { get; set; } = new List<KeyDefinitionEntity>();

    public DataSchema ToDomain()
    {
        return new DataSchema
        {
            Id = Id,
            TenantId = TenantId,
            DataModelId = DataModelId,
            Role = Role,
            Key = Key,
            Version = Version,
            Status = Status,
            Description = Description,
            CreatedAt = CreatedAt,
            PublishedAt = PublishedAt
        };
    }

    public static DataSchemaEntity FromDomain(DataSchema schema)
    {
        return new DataSchemaEntity
        {
            Id = schema.Id,
            TenantId = schema.TenantId,
            DataModelId = schema.DataModelId,
            Role = schema.Role,
            Key = schema.Key,
            Version = schema.Version,
            Status = schema.Status,
            Description = schema.Description,
            CreatedAt = schema.CreatedAt,
            PublishedAt = schema.PublishedAt
        };
    }
}
