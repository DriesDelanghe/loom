namespace Loom.Services.MasterDataConfiguration.Domain.Validation;

public class ValidationReference
{
    public Guid Id { get; set; }
    public Guid ParentValidationSpecId { get; set; }
    public string FieldPath { get; set; } = default!;
    public Guid ChildValidationSpecId { get; set; }
}

