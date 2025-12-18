namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record PublishTransformationSpecCommand(
    Guid TransformationSpecId,
    string PublishedBy
);

