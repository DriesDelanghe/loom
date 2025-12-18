using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Domain.Persistence;

public class WorkflowDefinitionEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<WorkflowVersionEntity> Versions { get; set; } = new List<WorkflowVersionEntity>();

    public WorkflowDefinition ToDomain()
    {
        return new WorkflowDefinition
        {
            Id = Id,
            TenantId = TenantId,
            Name = Name,
            Description = Description
        };
    }

    public static WorkflowDefinitionEntity FromDomain(WorkflowDefinition definition)
    {
        return new WorkflowDefinitionEntity
        {
            Id = definition.Id,
            TenantId = definition.TenantId,
            Name = definition.Name,
            Description = definition.Description,
            CreatedAt = DateTime.UtcNow
        };
    }
}


