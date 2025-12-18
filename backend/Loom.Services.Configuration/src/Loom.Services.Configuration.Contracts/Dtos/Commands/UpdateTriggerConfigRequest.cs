using System.Text.Json;

namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record UpdateTriggerConfigRequest(
    JsonDocument? Config
);

