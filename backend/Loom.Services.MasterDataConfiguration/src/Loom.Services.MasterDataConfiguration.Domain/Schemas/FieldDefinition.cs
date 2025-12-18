namespace Loom.Services.MasterDataConfiguration.Domain.Schemas;

public class FieldDefinition
{
    public Guid Id { get; set; }
    public Guid DataSchemaId { get; set; }
    public string Path { get; set; } = default!;
    public FieldType FieldType { get; set; }
    public ScalarType? ScalarType { get; set; }
    public Guid? ElementSchemaId { get; set; }
    public bool Required { get; set; }
    public string? Description { get; set; }
}

