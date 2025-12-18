using Loom.Services.MasterDataConfiguration.Domain.Transformation;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class TransformReferenceEntity
{
    public Guid Id { get; set; }
    public Guid ParentTransformationSpecId { get; set; }
    public string SourceFieldPath { get; set; } = default!;
    public string TargetFieldPath { get; set; } = default!;
    public Guid ChildTransformationSpecId { get; set; }

    public TransformationSpecEntity ParentTransformationSpec { get; set; } = null!;
    public TransformationSpecEntity ChildTransformationSpec { get; set; } = null!;

    public TransformReference ToDomain()
    {
        return new TransformReference
        {
            Id = Id,
            ParentTransformationSpecId = ParentTransformationSpecId,
            SourceFieldPath = SourceFieldPath,
            TargetFieldPath = TargetFieldPath,
            ChildTransformationSpecId = ChildTransformationSpecId
        };
    }

    public static TransformReferenceEntity FromDomain(TransformReference reference)
    {
        return new TransformReferenceEntity
        {
            Id = reference.Id,
            ParentTransformationSpecId = reference.ParentTransformationSpecId,
            SourceFieldPath = reference.SourceFieldPath,
            TargetFieldPath = reference.TargetFieldPath,
            ChildTransformationSpecId = reference.ChildTransformationSpecId
        };
    }
}
