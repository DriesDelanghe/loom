namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record UpdateSimpleTransformRuleCommand(
    Guid RuleId,
    string? SourcePath,
    string? TargetPath,
    Guid? ConverterId,
    bool? Required
);


