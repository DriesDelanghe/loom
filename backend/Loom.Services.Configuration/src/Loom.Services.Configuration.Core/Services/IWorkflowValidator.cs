namespace Loom.Services.Configuration.Core.Services;

public interface IWorkflowValidator
{
    Task<ValidationResult> ValidateAsync(Guid workflowVersionId, CancellationToken cancellationToken = default);
}

public record ValidationResult(
    bool IsValid,
    List<string> Errors,
    List<string> Warnings
);


