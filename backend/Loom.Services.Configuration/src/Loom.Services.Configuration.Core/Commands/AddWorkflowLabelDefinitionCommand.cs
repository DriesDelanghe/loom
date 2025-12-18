using Loom.Services.Configuration.Domain.Observability;

namespace Loom.Services.Configuration.Core.Commands;

public record AddWorkflowLabelDefinitionCommand(
    Guid WorkflowVersionId,
    string Key,
    LabelType Type,
    string? Description
);


