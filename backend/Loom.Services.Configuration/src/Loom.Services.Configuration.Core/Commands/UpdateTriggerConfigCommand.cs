using System.Text.Json;

namespace Loom.Services.Configuration.Core.Commands;

public record UpdateTriggerConfigCommand(
    Guid TriggerId,
    JsonDocument? Config
);


