namespace Loom.Services.MasterDataConfiguration.Domain.Transformation;

public class TransformGraphNode
{
    public Guid Id { get; set; }
    public Guid TransformationSpecId { get; set; }
    public string Key { get; set; } = default!;
    public TransformNodeType NodeType { get; set; }
    public string OutputType { get; set; } = default!; // JSON descriptor
    public string Config { get; set; } = default!; // JSONB
}


