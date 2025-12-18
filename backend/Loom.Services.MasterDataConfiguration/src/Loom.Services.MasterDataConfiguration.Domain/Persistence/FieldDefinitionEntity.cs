using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class FieldDefinitionEntity
{
    public Guid Id { get; set; }
    public Guid DataSchemaId { get; set; }
    public string Path { get; set; } = default!;
    public FieldType FieldType { get; set; }
    public ScalarType? ScalarType { get; set; }
    public Guid? ElementSchemaId { get; set; }
    public bool Required { get; set; }
    public string? Description { get; set; }

    public DataSchemaEntity DataSchema { get; set; } = null!;

    public FieldDefinition ToDomain()
    {
        return new FieldDefinition
        {
            Id = Id,
            DataSchemaId = DataSchemaId,
            Path = Path,
            FieldType = FieldType,
            ScalarType = ScalarType,
            ElementSchemaId = ElementSchemaId,
            Required = Required,
            Description = Description
        };
    }

    public static FieldDefinitionEntity FromDomain(FieldDefinition field)
    {
        return new FieldDefinitionEntity
        {
            Id = field.Id,
            DataSchemaId = field.DataSchemaId,
            Path = field.Path,
            FieldType = field.FieldType,
            ScalarType = field.ScalarType,
            ElementSchemaId = field.ElementSchemaId,
            Required = field.Required,
            Description = field.Description
        };
    }
}
