namespace Loom.Services.MasterDataConfiguration.Domain.Schemas;

public class KeyField
{
    public Guid Id { get; set; }
    public Guid KeyDefinitionId { get; set; }
    public string FieldPath { get; set; } = default!;
    public int Order { get; set; }
    public string? Normalization { get; set; }
}


