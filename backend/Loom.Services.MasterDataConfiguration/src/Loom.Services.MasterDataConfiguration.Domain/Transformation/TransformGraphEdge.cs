namespace Loom.Services.MasterDataConfiguration.Domain.Transformation;

public class TransformGraphEdge
{
    public Guid Id { get; set; }
    public Guid TransformationSpecId { get; set; }
    public Guid FromNodeId { get; set; }
    public Guid ToNodeId { get; set; }
    public string InputName { get; set; } = default!;
    public int Order { get; set; }
}

