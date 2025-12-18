using System.Text.Json;

namespace Loom.Services.Configuration.Core.Commands;

public record UpdateNodeConfigCommand(
    Guid NodeId,
    JsonDocument? Config
);


