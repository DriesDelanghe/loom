using System.Text.Json;
using Loom.Services.Configuration.Domain.Observability;

namespace Loom.Services.Configuration.Core.Commands;

public record AddWorkflowVariableCommand(
    Guid WorkflowVersionId,
    string Key,
    VariableType Type,
    JsonDocument? InitialValue,
    string? Description
);


