using Loom.Services.Configuration.Domain.Triggers;

namespace Loom.Services.Configuration.Domain.Persistence;

public class TriggerNodeBindingEntity
{
    public Guid Id { get; set; }
    public Guid TriggerBindingId { get; set; }
    public Guid EntryNodeId { get; set; }
    public int Order { get; set; }

    public TriggerBindingEntity TriggerBinding { get; set; } = null!;
    public NodeEntity EntryNode { get; set; } = null!;

    public TriggerNodeBinding ToDomain()
    {
        return new TriggerNodeBinding
        {
            Id = Id,
            TriggerBindingId = TriggerBindingId,
            EntryNodeId = EntryNodeId,
            Order = Order
        };
    }

    public static TriggerNodeBindingEntity FromDomain(TriggerNodeBinding binding)
    {
        return new TriggerNodeBindingEntity
        {
            Id = binding.Id,
            TriggerBindingId = binding.TriggerBindingId,
            EntryNodeId = binding.EntryNodeId,
            Order = binding.Order
        };
    }
}

