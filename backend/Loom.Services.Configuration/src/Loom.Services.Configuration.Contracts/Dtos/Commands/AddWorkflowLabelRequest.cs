using Loom.Services.Configuration.Domain.Observability;

namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record AddWorkflowLabelRequest(
    Guid WorkflowVersionId,
    string Key,
    LabelType Type,
    string? Description
);

