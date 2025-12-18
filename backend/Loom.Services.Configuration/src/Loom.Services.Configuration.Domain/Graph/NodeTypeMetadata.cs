using System.Collections.Generic;

namespace Loom.Services.Configuration.Domain.Graph;

public static class NodeTypeMetadata
{
    private static readonly Dictionary<NodeType, NodeTypeInfo> _metadata = new()
    {
        { NodeType.Action, new NodeTypeInfo(NodeCategory.Action, new[] { "Completed", "Failed" }, false) },
        { NodeType.Condition, new NodeTypeInfo(NodeCategory.Condition, new[] { "True", "False" }, false) },
        { NodeType.Validation, new NodeTypeInfo(NodeCategory.Validation, new[] { "Valid", "Invalid" }, false) },
        { NodeType.Split, new NodeTypeInfo(NodeCategory.Control, new[] { "Next" }, true) },
        { NodeType.Join, new NodeTypeInfo(NodeCategory.Control, new[] { "Joined" }, true) },
    };

    public static NodeCategory GetCategory(NodeType nodeType)
    {
        return _metadata.TryGetValue(nodeType, out var info) 
            ? info.Category 
            : throw new ArgumentException($"Unknown node type: {nodeType}", nameof(nodeType));
    }

    public static IReadOnlyList<string> GetAllowedOutcomes(NodeType nodeType)
    {
        return _metadata.TryGetValue(nodeType, out var info)
            ? info.AllowedOutcomes
            : throw new ArgumentException($"Unknown node type: {nodeType}", nameof(nodeType));
    }

    public static bool IsControlNode(NodeType nodeType)
    {
        return _metadata.TryGetValue(nodeType, out var info) && info.IsControl;
    }

    public static IReadOnlyList<string> GetRequiredOutcomes(NodeType nodeType)
    {
        var category = GetCategory(nodeType);
        return category switch
        {
            NodeCategory.Action => new[] { "Completed", "Failed" },
            NodeCategory.Condition => new[] { "True", "False" },
            NodeCategory.Validation => new[] { "Valid", "Invalid" },
            NodeCategory.Control => GetAllowedOutcomes(nodeType), // Control nodes use their specific outcomes
            _ => Array.Empty<string>()
        };
    }

    private class NodeTypeInfo
    {
        public NodeCategory Category { get; }
        public IReadOnlyList<string> AllowedOutcomes { get; }
        public bool IsControl { get; }

        public NodeTypeInfo(NodeCategory category, string[] allowedOutcomes, bool isControl)
        {
            Category = category;
            AllowedOutcomes = allowedOutcomes;
            IsControl = isControl;
        }
    }
}

