namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record PublishValidationSpecCommand(
    Guid ValidationSpecId,
    string PublishedBy
);


