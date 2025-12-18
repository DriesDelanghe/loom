namespace Loom.Services.MasterDataConfiguration.Domain.Schemas;

public class DataSchema
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
}


