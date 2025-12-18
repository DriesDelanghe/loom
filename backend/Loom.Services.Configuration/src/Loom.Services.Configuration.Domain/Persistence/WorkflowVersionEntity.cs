using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Domain.Persistence;

public class WorkflowVersionEntity
{
    public Guid Id { get; set; }
    public Guid DefinitionId { get; set; }
    public int Version { get; set; }
    public WorkflowStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? PublishedBy { get; set; }

    public WorkflowDefinitionEntity Definition { get; set; } = null!;
    public ICollection<NodeEntity> Nodes { get; set; } = new List<NodeEntity>();
    public ICollection<ConnectionEntity> Connections { get; set; } = new List<ConnectionEntity>();
    public ICollection<WorkflowVariableEntity> Variables { get; set; } = new List<WorkflowVariableEntity>();
    public ICollection<WorkflowLabelDefinitionEntity> Labels { get; set; } = new List<WorkflowLabelDefinitionEntity>();
    public WorkflowSettingsEntity? Settings { get; set; }
    public ICollection<TriggerBindingEntity> TriggerBindings { get; set; } = new List<TriggerBindingEntity>();

    public WorkflowVersion ToDomain()
    {
        return new WorkflowVersion
        {
            Id = Id,
            DefinitionId = DefinitionId,
            Version = Version,
            Status = Status,
            CreatedAt = CreatedAt,
            CreatedBy = CreatedBy,
            PublishedAt = PublishedAt
        };
    }

    public static WorkflowVersionEntity FromDomain(WorkflowVersion version)
    {
        return new WorkflowVersionEntity
        {
            Id = version.Id,
            DefinitionId = version.DefinitionId,
            Version = version.Version,
            Status = version.Status,
            CreatedAt = version.CreatedAt,
            CreatedBy = version.CreatedBy,
            PublishedAt = version.PublishedAt
        };
    }
}


