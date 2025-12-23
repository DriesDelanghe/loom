using Loom.Services.MasterDataConfiguration.Domain.Schemas;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Services;

/// <summary>
/// Internal helper for scalar element schemas.
/// Scalar arrays use a virtual schema concept where the element is a scalar value.
/// This is NOT stored in the database but used for validation and transformation resolution.
/// </summary>
public static class ScalarElementSchemaHelper
{
    /// <summary>
    /// Creates a virtual schema ID for a scalar type.
    /// This ID is deterministic and unique per scalar type.
    /// Format: "SCALAR-{ScalarType}" as a GUID-like identifier.
    /// </summary>
    public static Guid GetScalarElementSchemaId(ScalarType scalarType)
    {
        // Generate a deterministic GUID from the scalar type name
        var bytes = System.Text.Encoding.UTF8.GetBytes($"SCALAR-{scalarType}");
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        var guid = new Guid(hash.Take(16).ToArray());
        return guid;
    }

    /// <summary>
    /// Checks if a schema ID represents a scalar element schema.
    /// </summary>
    public static bool IsScalarElementSchemaId(Guid schemaId)
    {
        // Check if the GUID matches any scalar type pattern
        foreach (ScalarType scalarType in Enum.GetValues<ScalarType>())
        {
            if (GetScalarElementSchemaId(scalarType) == schemaId)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the scalar type from a scalar element schema ID.
    /// Returns null if the ID is not a scalar element schema.
    /// </summary>
    public static ScalarType? GetScalarTypeFromSchemaId(Guid schemaId)
    {
        foreach (ScalarType scalarType in Enum.GetValues<ScalarType>())
        {
            if (GetScalarElementSchemaId(scalarType) == schemaId)
            {
                return scalarType;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the element schema ID for an array field.
    /// For scalar arrays, returns the virtual scalar element schema ID.
    /// For object arrays, returns the ElementSchemaId.
    /// </summary>
    public static Guid? GetElementSchemaId(FieldDefinitionEntity field)
    {
        if (field.FieldType != FieldType.Array)
            return null;

        if (field.ScalarType.HasValue)
        {
            // Scalar array - return virtual schema ID
            return GetScalarElementSchemaId(field.ScalarType.Value);
        }
        else if (field.ElementSchemaId.HasValue)
        {
            // Object array - return actual schema ID
            return field.ElementSchemaId.Value;
        }

        return null;
    }
}

