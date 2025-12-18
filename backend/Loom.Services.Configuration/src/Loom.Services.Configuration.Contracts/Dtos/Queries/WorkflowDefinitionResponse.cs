namespace Loom.Services.Configuration.Contracts.Dtos.Queries;

public record WorkflowDefinitionResponse(
    Guid Id,
    string Name,
    bool HasPublishedVersion,
    int? LatestVersion
);

