namespace Loom.Services.Configuration.Core.Commands;

public record UnbindTriggerFromWorkflowVersionCommand(
    Guid TriggerBindingId
);


