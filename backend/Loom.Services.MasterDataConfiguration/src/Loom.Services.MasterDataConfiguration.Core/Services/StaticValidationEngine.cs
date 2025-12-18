using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;
using Loom.Services.MasterDataConfiguration.Domain.Validation;
using Loom.Services.MasterDataConfiguration.Domain.Transformation;

namespace Loom.Services.MasterDataConfiguration.Core.Services;

public class StaticValidationEngine : IStaticValidationEngine
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public StaticValidationEngine(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ValidationResult> ValidateSchemaAsync(Guid schemaId, CancellationToken cancellationToken = default)
    {
        return await ValidateSchemaAsync(schemaId, forPublish: false, cancellationToken);
    }

    public async Task<ValidationResult> ValidateSchemaAsync(Guid schemaId, bool forPublish, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        var schema = await _dbContext.DataSchemas
            .Include(s => s.Fields)
            .Include(s => s.KeyDefinitions)
            .ThenInclude(k => k.KeyFields)
            .FirstOrDefaultAsync(s => s.Id == schemaId, cancellationToken);

        if (schema == null)
        {
            errors.Add(new ValidationError { Field = "SchemaId", Message = $"Schema {schemaId} not found" });
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        // Master schemas must have a DataModelId when publishing, but it's optional when editing
        if (forPublish && schema.Role == SchemaRole.Master && !schema.DataModelId.HasValue)
        {
            errors.Add(new ValidationError { Field = "DataModelId", Message = "Master schemas must have a DataModelId before publishing" });
        }

        var fieldRegistry = await BuildFieldRegistryAsync(schemaId, cancellationToken);

        foreach (var field in schema.Fields)
        {
            if (field.FieldType == FieldType.Scalar && !field.ScalarType.HasValue)
            {
                errors.Add(new ValidationError { Field = $"Fields[{field.Path}].ScalarType", Message = "Scalar fields must have a ScalarType" });
            }

            if (field.FieldType != FieldType.Scalar && !field.ElementSchemaId.HasValue)
            {
                errors.Add(new ValidationError { Field = $"Fields[{field.Path}].ElementSchemaId", Message = "Object and Array fields must have an ElementSchemaId" });
            }

            if (field.ElementSchemaId.HasValue)
            {
                var elementSchema = await _dbContext.DataSchemas
                    .FirstOrDefaultAsync(s => s.Id == field.ElementSchemaId.Value, cancellationToken);

                if (elementSchema == null)
                {
                    errors.Add(new ValidationError { Field = $"Fields[{field.Path}].ElementSchemaId", Message = $"Referenced schema {field.ElementSchemaId.Value} not found" });
                }
                else
                {
                    // When publishing, referenced schemas must be Published
                    if (forPublish && elementSchema.Status != SchemaStatus.Published)
                    {
                        errors.Add(new ValidationError { Field = $"Fields[{field.Path}].ElementSchemaId", Message = $"Referenced schema '{elementSchema.Key}' (v{elementSchema.Version}) must be Published before this schema can be published" });
                    }
                    // Role must always match
                    if (elementSchema.Role != schema.Role)
                    {
                        errors.Add(new ValidationError { Field = $"Fields[{field.Path}].ElementSchemaId", Message = $"Referenced schema {field.ElementSchemaId.Value} must have the same Role as the parent schema" });
                    }
                }
            }
        }

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    public async Task<ValidationResult> ValidateValidationSpecAsync(Guid validationSpecId, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        var spec = await _dbContext.ValidationSpecs
            .Include(s => s.Rules)
            .Include(s => s.References)
            .FirstOrDefaultAsync(s => s.Id == validationSpecId, cancellationToken);

        if (spec == null)
        {
            errors.Add(new ValidationError { Field = "ValidationSpecId", Message = $"Validation spec {validationSpecId} not found" });
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        var schema = await _dbContext.DataSchemas
            .Include(s => s.Fields)
            .FirstOrDefaultAsync(s => s.Id == spec.DataSchemaId, cancellationToken);

        if (schema == null)
        {
            errors.Add(new ValidationError { Field = "DataSchemaId", Message = $"Data schema {spec.DataSchemaId} not found" });
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        var fieldRegistry = await BuildFieldRegistryAsync(spec.DataSchemaId, cancellationToken);

        foreach (var rule in spec.Rules)
        {
            try
            {
                var parameters = JsonDocument.Parse(rule.Parameters);
                ValidateValidationRule(rule, parameters, fieldRegistry, errors);
            }
            catch (JsonException ex)
            {
                errors.Add(new ValidationError { Field = $"Rules[{rule.Id}].Parameters", Message = $"Invalid JSON: {ex.Message}" });
            }
        }

        foreach (var reference in spec.References)
        {
            if (!fieldRegistry.ContainsKey(reference.FieldPath))
            {
                errors.Add(new ValidationError { Field = $"References[{reference.Id}].FieldPath", Message = $"Field path '{reference.FieldPath}' does not exist in schema" });
            }

            var childSpec = await _dbContext.ValidationSpecs
                .Include(s => s.Rules)
                .FirstOrDefaultAsync(s => s.Id == reference.ChildValidationSpecId, cancellationToken);

            if (childSpec == null)
            {
                errors.Add(new ValidationError { Field = $"References[{reference.Id}].ChildValidationSpecId", Message = $"Child validation spec {reference.ChildValidationSpecId} not found" });
            }
            else if (childSpec.Status != SchemaStatus.Published)
            {
                errors.Add(new ValidationError { Field = $"References[{reference.Id}].ChildValidationSpecId", Message = $"Child validation spec {reference.ChildValidationSpecId} must be Published" });
            }
        }

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    public async Task<ValidationResult> ValidateTransformationSpecAsync(Guid transformationSpecId, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        var spec = await _dbContext.TransformationSpecs
            .Include(s => s.SimpleRules)
            .Include(s => s.GraphNodes)
            .Include(s => s.GraphEdges)
            .Include(s => s.OutputBindings)
            .Include(s => s.References)
            .FirstOrDefaultAsync(s => s.Id == transformationSpecId, cancellationToken);

        if (spec == null)
        {
            errors.Add(new ValidationError { Field = "TransformationSpecId", Message = $"Transformation spec {transformationSpecId} not found" });
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        var sourceSchema = await _dbContext.DataSchemas
            .Include(s => s.Fields)
            .FirstOrDefaultAsync(s => s.Id == spec.SourceSchemaId, cancellationToken);

        if (sourceSchema == null)
        {
            errors.Add(new ValidationError { Field = "SourceSchemaId", Message = $"Source schema {spec.SourceSchemaId} not found" });
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        var targetSchema = await _dbContext.DataSchemas
            .Include(s => s.Fields)
            .FirstOrDefaultAsync(s => s.Id == spec.TargetSchemaId, cancellationToken);

        if (targetSchema == null)
        {
            errors.Add(new ValidationError { Field = "TargetSchemaId", Message = $"Target schema {spec.TargetSchemaId} not found" });
            return new ValidationResult { IsValid = false, Errors = errors };
        }

        var sourceFieldRegistry = await BuildFieldRegistryAsync(spec.SourceSchemaId, cancellationToken);
        var targetFieldRegistry = await BuildFieldRegistryAsync(spec.TargetSchemaId, cancellationToken);

        if (spec.Mode == TransformationMode.Simple)
        {
            foreach (var rule in spec.SimpleRules)
            {
                if (!sourceFieldRegistry.ContainsKey(rule.SourcePath))
                {
                    errors.Add(new ValidationError { Field = $"SimpleRules[{rule.Id}].SourcePath", Message = $"Source path '{rule.SourcePath}' does not exist in source schema" });
                }

                if (!targetFieldRegistry.ContainsKey(rule.TargetPath))
                {
                    errors.Add(new ValidationError { Field = $"SimpleRules[{rule.Id}].TargetPath", Message = $"Target path '{rule.TargetPath}' does not exist in target schema" });
                }
            }
        }
        else
        {
            var nodeIds = spec.GraphNodes.Select(n => n.Id).ToHashSet();

            foreach (var edge in spec.GraphEdges)
            {
                if (!nodeIds.Contains(edge.FromNodeId))
                {
                    errors.Add(new ValidationError { Field = $"GraphEdges[{edge.Id}].FromNodeId", Message = $"Source node {edge.FromNodeId} does not exist" });
                }

                if (!nodeIds.Contains(edge.ToNodeId))
                {
                    errors.Add(new ValidationError { Field = $"GraphEdges[{edge.Id}].ToNodeId", Message = $"Target node {edge.ToNodeId} does not exist" });
                }
            }

            if (!IsAcyclic(spec.GraphNodes, spec.GraphEdges))
            {
                errors.Add(new ValidationError { Field = "Graph", Message = "Transformation graph must be acyclic" });
            }

            foreach (var binding in spec.OutputBindings)
            {
                if (!nodeIds.Contains(binding.FromNodeId))
                {
                    errors.Add(new ValidationError { Field = $"OutputBindings[{binding.Id}].FromNodeId", Message = $"Source node {binding.FromNodeId} does not exist" });
                }

                if (!targetFieldRegistry.ContainsKey(binding.TargetPath))
                {
                    errors.Add(new ValidationError { Field = $"OutputBindings[{binding.Id}].TargetPath", Message = $"Target path '{binding.TargetPath}' does not exist in target schema" });
                }
            }
        }

        foreach (var reference in spec.References)
        {
            if (!sourceFieldRegistry.ContainsKey(reference.SourceFieldPath))
            {
                errors.Add(new ValidationError { Field = $"References[{reference.Id}].SourceFieldPath", Message = $"Source field path '{reference.SourceFieldPath}' does not exist in source schema" });
            }

            if (!targetFieldRegistry.ContainsKey(reference.TargetFieldPath))
            {
                errors.Add(new ValidationError { Field = $"References[{reference.Id}].TargetFieldPath", Message = $"Target field path '{reference.TargetFieldPath}' does not exist in target schema" });
            }

            var childSpec = await _dbContext.TransformationSpecs
                .FirstOrDefaultAsync(s => s.Id == reference.ChildTransformationSpecId, cancellationToken);

            if (childSpec == null)
            {
                errors.Add(new ValidationError { Field = $"References[{reference.Id}].ChildTransformationSpecId", Message = $"Child transformation spec {reference.ChildTransformationSpecId} not found" });
            }
            else if (childSpec.Status != SchemaStatus.Published)
            {
                errors.Add(new ValidationError { Field = $"References[{reference.Id}].ChildTransformationSpecId", Message = $"Child transformation spec {reference.ChildTransformationSpecId} must be Published" });
            }
        }

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }

    private async Task<Dictionary<string, FieldDefinitionEntity>> BuildFieldRegistryAsync(Guid schemaId, CancellationToken cancellationToken)
    {
        var fields = await _dbContext.FieldDefinitions
            .Where(f => f.DataSchemaId == schemaId)
            .ToListAsync(cancellationToken);

        var registry = new Dictionary<string, FieldDefinitionEntity>();

        foreach (var field in fields)
        {
            registry[field.Path] = field;
        }

        return registry;
    }

    private static void ValidateValidationRule(ValidationRuleEntity rule, JsonDocument parameters, Dictionary<string, FieldDefinitionEntity> fieldRegistry, List<ValidationError> errors)
    {
        switch (rule.RuleType)
        {
            case RuleType.Field:
                if (!parameters.RootElement.TryGetProperty("fieldPath", out var fieldPathElement))
                {
                    errors.Add(new ValidationError { Field = $"Rules[{rule.Id}].Parameters.fieldPath", Message = "Field rule must have 'fieldPath' parameter" });
                    return;
                }

                var fieldPath = fieldPathElement.GetString();
                if (string.IsNullOrEmpty(fieldPath))
                {
                    errors.Add(new ValidationError { Field = $"Rules[{rule.Id}].Parameters.fieldPath", Message = "Field path cannot be empty" });
                    return;
                }

                if (!fieldRegistry.ContainsKey(fieldPath))
                {
                    errors.Add(new ValidationError { Field = $"Rules[{rule.Id}].Parameters.fieldPath", Message = $"Field path '{fieldPath}' does not exist in schema" });
                }
                break;

            case RuleType.CrossField:
                if (!parameters.RootElement.TryGetProperty("fieldPaths", out var fieldPathsElement))
                {
                    errors.Add(new ValidationError { Field = $"Rules[{rule.Id}].Parameters.fieldPaths", Message = "CrossField rule must have 'fieldPaths' parameter" });
                    return;
                }

                foreach (var pathElement in fieldPathsElement.EnumerateArray())
                {
                    var path = pathElement.GetString();
                    if (string.IsNullOrEmpty(path))
                        continue;

                    if (!fieldRegistry.ContainsKey(path))
                    {
                        errors.Add(new ValidationError { Field = $"Rules[{rule.Id}].Parameters.fieldPaths", Message = $"Field path '{path}' does not exist in schema" });
                    }
                }
                break;

            case RuleType.Conditional:
                if (!parameters.RootElement.TryGetProperty("condition", out var conditionElement))
                {
                    errors.Add(new ValidationError { Field = $"Rules[{rule.Id}].Parameters.condition", Message = "Conditional rule must have 'condition' parameter" });
                }
                break;
        }
    }

    private static bool IsAcyclic(ICollection<TransformGraphNodeEntity> nodes, ICollection<TransformGraphEdgeEntity> edges)
    {
        var visited = new HashSet<Guid>();
        var recursionStack = new HashSet<Guid>();

        foreach (var node in nodes)
        {
            if (!visited.Contains(node.Id))
            {
                if (HasCycle(node.Id, edges, visited, recursionStack))
                    return false;
            }
        }

        return true;
    }

    private static bool HasCycle(Guid nodeId, ICollection<TransformGraphEdgeEntity> edges, HashSet<Guid> visited, HashSet<Guid> recursionStack)
    {
        visited.Add(nodeId);
        recursionStack.Add(nodeId);

        var outgoingEdges = edges.Where(e => e.FromNodeId == nodeId);
        foreach (var edge in outgoingEdges)
        {
            if (!visited.Contains(edge.ToNodeId))
            {
                if (HasCycle(edge.ToNodeId, edges, visited, recursionStack))
                    return true;
            }
            else if (recursionStack.Contains(edge.ToNodeId))
            {
                return true;
            }
        }

        recursionStack.Remove(nodeId);
        return false;
    }
}

