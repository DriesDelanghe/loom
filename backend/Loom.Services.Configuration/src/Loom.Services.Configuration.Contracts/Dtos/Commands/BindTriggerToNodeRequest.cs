namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record BindTriggerToNodeRequest(
    Guid TriggerBindingId,
    Guid EntryNodeId,
    int? Order
);

