namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddTransformOutputBindingCommand(
    Guid TransformationSpecId,
    string TargetPath,
    Guid FromNodeId
);

