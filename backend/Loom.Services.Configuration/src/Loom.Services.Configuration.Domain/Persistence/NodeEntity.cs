using System.Text.Json;
using Loom.Services.Configuration.Domain.Graph;

namespace Loom.Services.Configuration.Domain.Persistence;

public class NodeEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string Key { get; set; } = default!;
    public string? Name { get; set; }
    public NodeType Type { get; set; }
    public string? ConfigJson { get; set; }
    public DateTime CreatedAt { get; set; }

    public WorkflowVersionEntity WorkflowVersion { get; set; } = null!;

    public Node ToDomain()
    {
        return new Node
        {
            Id = Id,
            WorkflowVersionId = WorkflowVersionId,
            Key = Key,
            Name = Name,
            Type = Type,
            Config = ConfigJson != null ? JsonDocument.Parse(ConfigJson) : null,
            CreatedAt = CreatedAt
        };
    }

    public static NodeEntity FromDomain(Node node)
    {
        return new NodeEntity
        {
            Id = node.Id,
            WorkflowVersionId = node.WorkflowVersionId,
            Key = node.Key,
            Name = node.Name,
            Type = node.Type,
            ConfigJson = node.Config?.RootElement.GetRawText(),
            CreatedAt = node.CreatedAt
        };
    }
}


