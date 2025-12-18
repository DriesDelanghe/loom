using Loom.Services.Configuration.Domain.Observability;

namespace Loom.Services.Configuration.Domain.Persistence;

public class WorkflowLabelDefinitionEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string Key { get; set; } = default!;
    public LabelType Type { get; set; }
    public string? Description { get; set; }

    public WorkflowVersionEntity WorkflowVersion { get; set; } = null!;

    public WorkflowLabelDefinition ToDomain()
    {
        return new WorkflowLabelDefinition
        {
            Id = Id,
            WorkflowVersionId = WorkflowVersionId,
            Key = Key,
            Type = Type,
            Description = Description
        };
    }

    public static WorkflowLabelDefinitionEntity FromDomain(WorkflowLabelDefinition label)
    {
        return new WorkflowLabelDefinitionEntity
        {
            Id = label.Id,
            WorkflowVersionId = label.WorkflowVersionId,
            Key = label.Key,
            Type = label.Type,
            Description = label.Description
        };
    }
}


