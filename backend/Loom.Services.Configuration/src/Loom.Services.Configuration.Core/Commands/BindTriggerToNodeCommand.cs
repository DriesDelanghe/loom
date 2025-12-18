namespace Loom.Services.Configuration.Core.Commands;

public record BindTriggerToNodeCommand(
    Guid TriggerBindingId,
    Guid EntryNodeId,
    int? Order = null
);

