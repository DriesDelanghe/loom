using System.Text.Json;
using Loom.Services.Configuration.Domain.Persistence;

namespace Loom.Services.Configuration.Domain.Graph;

public static class NodeConfigValidator
{
    public static void ValidateConfig(NodeEntity node, List<string> errors)
    {
        if (node.ConfigJson == null)
        {
            // Config is optional for most nodes
            return;
        }

        try
        {
            var config = JsonDocument.Parse(node.ConfigJson);
            var root = config.RootElement;

            switch (node.Type)
            {
                case NodeType.Split:
                    ValidateSplitConfig(root, node.Key, errors);
                    break;
                case NodeType.Join:
                    ValidateJoinConfig(root, node.Key, errors);
                    break;
                // Other node types don't have strict config requirements yet
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Node '{node.Key}' has invalid JSON config: {ex.Message}");
        }
    }

    private static void ValidateSplitConfig(JsonElement config, string nodeKey, List<string> errors)
    {
        if (config.ValueKind != JsonValueKind.Object)
        {
            errors.Add($"Split node '{nodeKey}' config must be an object");
            return;
        }

        if (config.TryGetProperty("maxParallelism", out var maxParallelismProp))
        {
            if (maxParallelismProp.ValueKind != JsonValueKind.Number || !maxParallelismProp.TryGetInt32(out var maxParallelism) || maxParallelism < 1)
            {
                errors.Add($"Split node '{nodeKey}' config 'maxParallelism' must be a positive integer");
            }
        }
    }

    private static void ValidateJoinConfig(JsonElement config, string nodeKey, List<string> errors)
    {
        if (config.ValueKind != JsonValueKind.Object)
        {
            errors.Add($"Join node '{nodeKey}' config must be an object");
            return;
        }

        if (config.TryGetProperty("joinType", out var joinTypeProp))
        {
            if (joinTypeProp.ValueKind != JsonValueKind.String)
            {
                errors.Add($"Join node '{nodeKey}' config 'joinType' must be a string");
            }
            else
            {
                var joinType = joinTypeProp.GetString();
                if (joinType != "All" && joinType != "Any")
                {
                    errors.Add($"Join node '{nodeKey}' config 'joinType' must be either 'All' or 'Any'");
                }
            }
        }

        if (config.TryGetProperty("cancelRemaining", out var cancelRemainingProp))
        {
            if (cancelRemainingProp.ValueKind != JsonValueKind.True && cancelRemainingProp.ValueKind != JsonValueKind.False)
            {
                errors.Add($"Join node '{nodeKey}' config 'cancelRemaining' must be a boolean");
            }
        }
    }
}

