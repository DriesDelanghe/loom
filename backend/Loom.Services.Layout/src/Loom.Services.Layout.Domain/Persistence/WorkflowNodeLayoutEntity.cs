namespace Loom.Services.Layout.Domain.Persistence;

public class WorkflowNodeLayoutEntity
{
    public Guid TenantId { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string NodeKey { get; set; } = default!;
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public DateTime UpdatedAt { get; set; }
}

