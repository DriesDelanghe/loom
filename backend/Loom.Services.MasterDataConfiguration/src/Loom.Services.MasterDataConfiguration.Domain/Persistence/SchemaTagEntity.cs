using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class SchemaTagEntity
{
    public Guid Id { get; set; }
    public Guid DataSchemaId { get; set; }
    public string Tag { get; set; } = default!;

    public DataSchemaEntity DataSchema { get; set; } = null!;

    public SchemaTag ToDomain()
    {
        return new SchemaTag
        {
            Id = Id,
            DataSchemaId = DataSchemaId,
            Tag = Tag
        };
    }

    public static SchemaTagEntity FromDomain(SchemaTag tag)
    {
        return new SchemaTagEntity
        {
            Id = tag.Id,
            DataSchemaId = tag.DataSchemaId,
            Tag = tag.Tag
        };
    }
}
