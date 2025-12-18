using Loom.Services.MasterDataConfiguration.Domain.Validation;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class ValidationReferenceEntity
{
    public Guid Id { get; set; }
    public Guid ParentValidationSpecId { get; set; }
    public string FieldPath { get; set; } = default!;
    public Guid ChildValidationSpecId { get; set; }

    public ValidationSpecEntity ParentValidationSpec { get; set; } = null!;
    public ValidationSpecEntity ChildValidationSpec { get; set; } = null!;

    public ValidationReference ToDomain()
    {
        return new ValidationReference
        {
            Id = Id,
            ParentValidationSpecId = ParentValidationSpecId,
            FieldPath = FieldPath,
            ChildValidationSpecId = ChildValidationSpecId
        };
    }

    public static ValidationReferenceEntity FromDomain(ValidationReference reference)
    {
        return new ValidationReferenceEntity
        {
            Id = reference.Id,
            ParentValidationSpecId = reference.ParentValidationSpecId,
            FieldPath = reference.FieldPath,
            ChildValidationSpecId = reference.ChildValidationSpecId
        };
    }
}
