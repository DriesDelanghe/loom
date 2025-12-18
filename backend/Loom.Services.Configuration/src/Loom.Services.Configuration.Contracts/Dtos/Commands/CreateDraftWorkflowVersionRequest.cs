namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record CreateDraftWorkflowVersionRequest(
    Guid WorkflowDefinitionId,
    string CreatedBy
);

