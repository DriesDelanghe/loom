namespace Loom.Services.Configuration.Domain.Triggers;

public class TriggerBinding
{
    public Guid Id { get; set; }
    public Guid TriggerId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public bool Enabled { get; set; }
    public int? Priority { get; set; }
}