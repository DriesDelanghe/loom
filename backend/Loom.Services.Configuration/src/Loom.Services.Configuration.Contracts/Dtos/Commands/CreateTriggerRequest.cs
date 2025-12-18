using System.Text.Json;
using Loom.Services.Configuration.Domain.Triggers;

namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record CreateTriggerRequest(
    Guid TenantId,
    TriggerType Type,
    JsonDocument? Config
);

