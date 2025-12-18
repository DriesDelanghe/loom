using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Domain.Validation;

public class ValidationSpec
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid DataSchemaId { get; set; }
    public int Version { get; set; }
    public SchemaStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}


