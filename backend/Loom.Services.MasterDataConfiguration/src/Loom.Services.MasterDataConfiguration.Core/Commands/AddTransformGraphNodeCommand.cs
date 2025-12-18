namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddTransformGraphNodeCommand(
    Guid TransformationSpecId,
    string Key,
    Domain.Transformation.TransformNodeType NodeType,
    string OutputType,
    string Config
);

