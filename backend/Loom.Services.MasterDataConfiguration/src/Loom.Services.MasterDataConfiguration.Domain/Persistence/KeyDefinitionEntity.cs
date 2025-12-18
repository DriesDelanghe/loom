using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class KeyDefinitionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid DataSchemaId { get; set; }
    public string Name { get; set; } = default!;
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }

    public DataSchemaEntity DataSchema { get; set; } = null!;
    public ICollection<KeyFieldEntity> KeyFields { get; set; } = new List<KeyFieldEntity>();

    public KeyDefinition ToDomain()
    {
        return new KeyDefinition
        {
            Id = Id,
            TenantId = TenantId,
            DataSchemaId = DataSchemaId,
            Name = Name,
            IsPrimary = IsPrimary,
            CreatedAt = CreatedAt
        };
    }

    public static KeyDefinitionEntity FromDomain(KeyDefinition key)
    {
        return new KeyDefinitionEntity
        {
            Id = key.Id,
            TenantId = key.TenantId,
            DataSchemaId = key.DataSchemaId,
            Name = key.Name,
            IsPrimary = key.IsPrimary,
            CreatedAt = key.CreatedAt
        };
    }
}
