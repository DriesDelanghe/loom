using Loom.Services.MasterDataConfiguration.Domain.Transformation;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class TransformGraphNodeEntity
{
    public Guid Id { get; set; }
    public Guid TransformationSpecId { get; set; }
    public string Key { get; set; } = default!;
    public TransformNodeType NodeType { get; set; }
    public string OutputType { get; set; } = default!; // JSON descriptor
    public string Config { get; set; } = default!; // JSONB

    public TransformationSpecEntity TransformationSpec { get; set; } = null!;

    public TransformGraphNode ToDomain()
    {
        return new TransformGraphNode
        {
            Id = Id,
            TransformationSpecId = TransformationSpecId,
            Key = Key,
            NodeType = NodeType,
            OutputType = OutputType,
            Config = Config
        };
    }

    public static TransformGraphNodeEntity FromDomain(TransformGraphNode node)
    {
        return new TransformGraphNodeEntity
        {
            Id = node.Id,
            TransformationSpecId = node.TransformationSpecId,
            Key = node.Key,
            NodeType = node.NodeType,
            OutputType = node.OutputType,
            Config = node.Config
        };
    }
}
