namespace Loom.Services.Configuration.Core.Commands;

public record CreateDraftWorkflowVersionCommand(
    Guid WorkflowDefinitionId,
    string CreatedBy
);


