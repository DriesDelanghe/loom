using System.Text.Json;

namespace Loom.Services.Configuration.Domain.Observability;

public class WorkflowVariable
{
    public Guid Id { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string Key { get; set; } = default!;
    public VariableType Type { get; set; }
    public JsonDocument? InitialValue { get; set; }
    public string? Description { get; set; }
}