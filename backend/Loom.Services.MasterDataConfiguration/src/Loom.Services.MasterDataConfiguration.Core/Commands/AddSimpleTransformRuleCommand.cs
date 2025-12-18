namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddSimpleTransformRuleCommand(
    Guid TransformationSpecId,
    string SourcePath,
    string TargetPath,
    Guid? ConverterId,
    bool Required,
    int Order
);


