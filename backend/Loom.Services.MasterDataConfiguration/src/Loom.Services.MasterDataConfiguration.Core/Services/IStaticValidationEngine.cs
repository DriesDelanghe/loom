namespace Loom.Services.MasterDataConfiguration.Core.Services;

public interface IStaticValidationEngine
{
    Task<ValidationResult> ValidateSchemaAsync(Guid schemaId, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateSchemaAsync(Guid schemaId, bool forPublish, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateValidationSpecAsync(Guid validationSpecId, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateTransformationSpecAsync(Guid transformationSpecId, CancellationToken cancellationToken = default);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public IReadOnlyList<ValidationError> Errors { get; set; } = Array.Empty<ValidationError>();
}

public class ValidationError
{
    public string Field { get; set; } = default!;
    public string Message { get; set; } = default!;
}

