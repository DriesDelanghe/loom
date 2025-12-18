namespace Loom.Services.Configuration.Domain.Triggers;

public class TriggerNodeBinding
{
    public Guid Id { get; set; }
    public Guid TriggerBindingId { get; set; }
    public Guid EntryNodeId { get; set; }
    public int Order { get; set; }
}

