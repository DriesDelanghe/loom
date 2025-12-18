using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class KeyFieldEntity
{
    public Guid Id { get; set; }
    public Guid KeyDefinitionId { get; set; }
    public string FieldPath { get; set; } = default!;
    public int Order { get; set; }
    public string? Normalization { get; set; }

    public KeyDefinitionEntity KeyDefinition { get; set; } = null!;

    public KeyField ToDomain()
    {
        return new KeyField
        {
            Id = Id,
            KeyDefinitionId = KeyDefinitionId,
            FieldPath = FieldPath,
            Order = Order,
            Normalization = Normalization
        };
    }

    public static KeyFieldEntity FromDomain(KeyField field)
    {
        return new KeyFieldEntity
        {
            Id = field.Id,
            KeyDefinitionId = field.KeyDefinitionId,
            FieldPath = field.FieldPath,
            Order = field.Order,
            Normalization = field.Normalization
        };
    }
}
