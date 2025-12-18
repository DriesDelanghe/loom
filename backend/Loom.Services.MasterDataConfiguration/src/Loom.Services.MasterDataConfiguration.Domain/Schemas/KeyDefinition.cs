namespace Loom.Services.MasterDataConfiguration.Domain.Schemas;

public class KeyDefinition
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid DataSchemaId { get; set; }
    public string Name { get; set; } = default!;
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
}

