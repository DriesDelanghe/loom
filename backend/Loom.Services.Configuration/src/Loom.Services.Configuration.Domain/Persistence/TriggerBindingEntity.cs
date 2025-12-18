using Loom.Services.Configuration.Domain.Triggers;

namespace Loom.Services.Configuration.Domain.Persistence;

public class TriggerBindingEntity
{
    public Guid Id { get; set; }
    public Guid TriggerId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public bool Enabled { get; set; }
    public int? Priority { get; set; }

    public TriggerEntity Trigger { get; set; } = null!;
    public WorkflowVersionEntity WorkflowVersion { get; set; } = null!;
    public ICollection<TriggerNodeBindingEntity> NodeBindings { get; set; } = new List<TriggerNodeBindingEntity>();

    public TriggerBinding ToDomain()
    {
        return new TriggerBinding
        {
            Id = Id,
            TriggerId = TriggerId,
            WorkflowVersionId = WorkflowVersionId,
            Enabled = Enabled,
            Priority = Priority
        };
    }

    public static TriggerBindingEntity FromDomain(TriggerBinding binding)
    {
        return new TriggerBindingEntity
        {
            Id = binding.Id,
            TriggerId = binding.TriggerId,
            WorkflowVersionId = binding.WorkflowVersionId,
            Enabled = binding.Enabled,
            Priority = binding.Priority
        };
    }
}


