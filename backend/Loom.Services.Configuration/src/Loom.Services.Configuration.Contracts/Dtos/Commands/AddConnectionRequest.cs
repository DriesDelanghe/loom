namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record AddConnectionRequest(
    Guid WorkflowVersionId,
    Guid FromNodeId,
    Guid ToNodeId,
    string Outcome,
    int? Order
);

