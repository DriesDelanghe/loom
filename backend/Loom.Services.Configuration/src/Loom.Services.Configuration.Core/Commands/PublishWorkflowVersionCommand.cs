namespace Loom.Services.Configuration.Core.Commands;

public record PublishWorkflowVersionCommand(
    Guid WorkflowVersionId,
    string PublishedBy
);


