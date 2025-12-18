namespace Loom.Services.MasterDataConfiguration.Domain.Transformation;

public class TransformOutputBinding
{
    public Guid Id { get; set; }
    public Guid TransformationSpecId { get; set; }
    public string TargetPath { get; set; } = default!;
    public Guid FromNodeId { get; set; }
}

