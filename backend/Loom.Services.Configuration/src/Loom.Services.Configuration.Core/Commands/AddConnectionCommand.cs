namespace Loom.Services.Configuration.Core.Commands;

public record AddConnectionCommand(
    Guid WorkflowVersionId,
    Guid FromNodeId,
    Guid ToNodeId,
    string Outcome,
    int? Order
);


