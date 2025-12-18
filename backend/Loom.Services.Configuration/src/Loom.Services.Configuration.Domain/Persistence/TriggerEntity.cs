using System.Text.Json;
using Loom.Services.Configuration.Domain.Triggers;

namespace Loom.Services.Configuration.Domain.Persistence;

public class TriggerEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public TriggerType Type { get; set; }
    public string? ConfigJson { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<TriggerBindingEntity> Bindings { get; set; } = new List<TriggerBindingEntity>();

    public Trigger ToDomain()
    {
        return new Trigger
        {
            Id = Id,
            TenantId = TenantId,
            Type = Type,
            Config = ConfigJson != null ? JsonDocument.Parse(ConfigJson) : null,
            CreatedAt = CreatedAt
        };
    }

    public static TriggerEntity FromDomain(Trigger trigger)
    {
        return new TriggerEntity
        {
            Id = trigger.Id,
            TenantId = trigger.TenantId,
            Type = trigger.Type,
            ConfigJson = trigger.Config?.RootElement.GetRawText(),
            CreatedAt = trigger.CreatedAt
        };
    }
}


