namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddValidationReferenceCommand(
    Guid ParentValidationSpecId,
    string FieldPath,
    Guid ChildValidationSpecId
);

