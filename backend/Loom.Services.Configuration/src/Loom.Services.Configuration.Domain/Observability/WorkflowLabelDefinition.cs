namespace Loom.Services.Configuration.Domain.Observability;

public class WorkflowLabelDefinition
{
    public Guid Id { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string Key { get; set; } = default!;
    public LabelType Type { get; set; }
    public string? Description { get; set; }
}