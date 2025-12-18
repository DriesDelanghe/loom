namespace Loom.Services.Configuration.Contracts.Dtos.Queries;

public record WorkflowVersionForTriggerResponse(
    Guid WorkflowVersionId,
    Guid TenantId,
    int? Priority
);

