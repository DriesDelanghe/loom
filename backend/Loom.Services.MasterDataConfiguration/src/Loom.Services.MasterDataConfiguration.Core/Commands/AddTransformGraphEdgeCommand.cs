namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddTransformGraphEdgeCommand(
    Guid TransformationSpecId,
    Guid FromNodeId,
    Guid ToNodeId,
    string InputName,
    int Order
);


