namespace Loom.Services.MasterDataConfiguration.Domain.Schemas;

public class SchemaTag
{
    public Guid Id { get; set; }
    public Guid DataSchemaId { get; set; }
    public string Tag { get; set; } = default!;
}

