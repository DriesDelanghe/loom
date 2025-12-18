using System.Text.Json;
using Loom.Services.Configuration.Domain.Graph;

namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record AddNodeRequest(
    Guid WorkflowVersionId,
    string Key,
    string? Name,
    NodeType Type,
    JsonDocument? Config
);

