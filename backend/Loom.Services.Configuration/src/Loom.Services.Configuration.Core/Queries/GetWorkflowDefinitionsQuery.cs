namespace Loom.Services.Configuration.Core.Queries;

public record GetWorkflowDefinitionsQuery(
    Guid TenantId
);

public record WorkflowDefinitionDto(
    Guid Id,
    string Name,
    bool HasPublishedVersion,
    int? LatestVersion
);


