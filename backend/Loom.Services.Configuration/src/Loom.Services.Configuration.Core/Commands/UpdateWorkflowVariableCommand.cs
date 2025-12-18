using System.Text.Json;
using Loom.Services.Configuration.Domain.Observability;

namespace Loom.Services.Configuration.Core.Commands;

public record UpdateWorkflowVariableCommand(
    Guid VariableId,
    VariableType? Type,
    JsonDocument? InitialValue,
    string? Description
);


