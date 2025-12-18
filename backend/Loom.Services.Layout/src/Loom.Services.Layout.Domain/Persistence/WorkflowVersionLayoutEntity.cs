namespace Loom.Services.Layout.Domain.Persistence;

public class WorkflowVersionLayoutEntity
{
    public Guid TenantId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public DateTime UpdatedAt { get; set; }
}

