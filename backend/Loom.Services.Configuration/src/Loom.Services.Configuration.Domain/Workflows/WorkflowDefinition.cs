namespace Loom.Services.Configuration.Domain.Workflows;

public class WorkflowDefinition
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

}