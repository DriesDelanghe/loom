namespace Loom.Services.Configuration.Core.Commands;

public record BindTriggerToWorkflowVersionCommand(
    Guid TriggerId,
    Guid WorkflowVersionId,
    int? Priority,
    bool Enabled
);


