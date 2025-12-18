using System.Text.Json;
using Loom.Services.Configuration.Domain.Triggers;

namespace Loom.Services.Configuration.Core.Commands;

public record CreateTriggerCommand(
    Guid TenantId,
    TriggerType Type,
    JsonDocument? Config
);


