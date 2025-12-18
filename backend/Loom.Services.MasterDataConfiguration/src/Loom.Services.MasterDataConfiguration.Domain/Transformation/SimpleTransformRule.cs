namespace Loom.Services.MasterDataConfiguration.Domain.Transformation;

public class SimpleTransformRule
{
    public Guid Id { get; set; }
    public Guid TransformationSpecId { get; set; }
    public string SourcePath { get; set; } = default!;
    public string TargetPath { get; set; } = default!;
    public Guid? ConverterId { get; set; }
    public bool Required { get; set; }
    public int Order { get; set; }
}

