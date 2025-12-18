using System.Text.Json;
using Loom.Services.Configuration.Domain.Observability;

namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record UpdateWorkflowVariableRequest(
    VariableType? Type,
    JsonDocument? InitialValue,
    string? Description
);

