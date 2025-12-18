namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record BindTriggerRequest(
    Guid TriggerId,
    Guid WorkflowVersionId,
    int? Priority,
    bool Enabled
);

