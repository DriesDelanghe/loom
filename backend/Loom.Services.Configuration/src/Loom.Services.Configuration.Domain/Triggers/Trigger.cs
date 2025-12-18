using System.Text.Json;

namespace Loom.Services.Configuration.Domain.Triggers;

public class Trigger
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public TriggerType Type { get; set; }
    public JsonDocument? Config { get; set; }
    public DateTime CreatedAt { get; set; }
}