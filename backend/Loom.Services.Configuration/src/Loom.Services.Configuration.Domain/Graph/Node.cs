using System.Text.Json;

namespace Loom.Services.Configuration.Domain.Graph;

public class Node
{
    public Guid Id { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string Key { get; set; } = default!;
    public string? Name { get; set; }
    public NodeType Type { get; set; }
    public JsonDocument? Config { get; set; }
    public DateTime CreatedAt { get; set; }
}