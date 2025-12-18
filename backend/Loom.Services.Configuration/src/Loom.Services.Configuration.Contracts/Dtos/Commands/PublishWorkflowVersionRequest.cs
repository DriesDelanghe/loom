namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record PublishWorkflowVersionRequest(
    Guid WorkflowVersionId,
    string PublishedBy
);

