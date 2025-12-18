namespace Loom.Services.MasterDataConfiguration.Domain.Transformation;

public class TransformReference
{
    public Guid Id { get; set; }
    public Guid ParentTransformationSpecId { get; set; }
    public string SourceFieldPath { get; set; } = default!;
    public string TargetFieldPath { get; set; } = default!;
    public Guid ChildTransformationSpecId { get; set; }
}


