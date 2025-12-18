using Loom.Services.MasterDataConfiguration.Domain.Transformation;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class TransformGraphEdgeEntity
{
    public Guid Id { get; set; }
    public Guid TransformationSpecId { get; set; }
    public Guid FromNodeId { get; set; }
    public Guid ToNodeId { get; set; }
    public string InputName { get; set; } = default!;
    public int Order { get; set; }

    public TransformationSpecEntity TransformationSpec { get; set; } = null!;

    public TransformGraphEdge ToDomain()
    {
        return new TransformGraphEdge
        {
            Id = Id,
            TransformationSpecId = TransformationSpecId,
            FromNodeId = FromNodeId,
            ToNodeId = ToNodeId,
            InputName = InputName,
            Order = Order
        };
    }

    public static TransformGraphEdgeEntity FromDomain(TransformGraphEdge edge)
    {
        return new TransformGraphEdgeEntity
        {
            Id = edge.Id,
            TransformationSpecId = edge.TransformationSpecId,
            FromNodeId = edge.FromNodeId,
            ToNodeId = edge.ToNodeId,
            InputName = edge.InputName,
            Order = edge.Order
        };
    }
}
