using System.Text.Json;
using Loom.Services.Configuration.Domain.Graph;

namespace Loom.Services.Configuration.Core.Commands;

public record AddNodeCommand(
    Guid WorkflowVersionId,
    string Key,
    string? Name,
    NodeType Type,
    JsonDocument? Config
);


