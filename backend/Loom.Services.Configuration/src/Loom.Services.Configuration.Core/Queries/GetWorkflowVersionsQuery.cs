namespace Loom.Services.Configuration.Core.Queries;

public record GetWorkflowVersionsQuery(
    Guid WorkflowDefinitionId
);

public record WorkflowVersionDto(
    Guid Id,
    int Version,
    string Status,
    DateTime CreatedAt,
    DateTime? PublishedAt
);


