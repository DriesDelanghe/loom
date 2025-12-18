using System.Text.Json;

namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record UpdateNodeConfigRequest(
    Guid NodeId,
    JsonDocument? Config
);

