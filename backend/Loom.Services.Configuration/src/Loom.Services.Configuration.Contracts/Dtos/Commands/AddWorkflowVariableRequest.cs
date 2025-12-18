using System.Text.Json;
using Loom.Services.Configuration.Domain.Observability;

namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record AddWorkflowVariableRequest(
    Guid WorkflowVersionId,
    string Key,
    VariableType Type,
    JsonDocument? InitialValue,
    string? Description
);

