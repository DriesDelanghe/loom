namespace Loom.Services.Configuration.Domain.Workflows;

public class WorkflowSettings
{
    public Guid WorkflowVersionId { get; set; }
    public int? MaxNodeExecutions { get; set; }
    public int? MaxDurationSeconds { get; set; }
}