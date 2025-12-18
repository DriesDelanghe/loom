namespace Loom.Services.Configuration.Core.Queries;

public record ValidateWorkflowVersionQuery(
    Guid WorkflowVersionId
);

public record ValidationResultDto(
    bool IsValid,
    List<string> Errors,
    List<string> Warnings
);


