namespace Loom.Services.Configuration.Domain.Workflows;

public class WorkflowVersion
{
    public Guid Id { get; set; }
    public Guid DefinitionId { get; set; }
    public int Version { get; set; }
    public WorkflowStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
}