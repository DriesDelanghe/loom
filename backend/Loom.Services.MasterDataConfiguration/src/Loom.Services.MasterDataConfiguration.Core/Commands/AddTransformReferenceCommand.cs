namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddTransformReferenceCommand(
    Guid ParentTransformationSpecId,
    string SourceFieldPath,
    string TargetFieldPath,
    Guid ChildTransformationSpecId
);

