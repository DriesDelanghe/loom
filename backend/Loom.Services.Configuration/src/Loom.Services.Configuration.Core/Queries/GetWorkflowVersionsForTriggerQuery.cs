namespace Loom.Services.Configuration.Core.Queries;

public record GetWorkflowVersionsForTriggerQuery(
    Guid TriggerId
);

public record WorkflowVersionForTriggerDto(
    Guid WorkflowVersionId,
    Guid TenantId,
    int? Priority
);


