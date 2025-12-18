using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Core.Services;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class ValidateSchemaQueryHandler : IQueryHandler<ValidateSchemaQuery, ValidationResult>
{
    private readonly IStaticValidationEngine _validationEngine;

    public ValidateSchemaQueryHandler(IStaticValidationEngine validationEngine)
    {
        _validationEngine = validationEngine;
    }

    public async Task<ValidationResult> HandleAsync(ValidateSchemaQuery query, CancellationToken cancellationToken = default)
    {
        return await _validationEngine.ValidateSchemaAsync(query.SchemaId, cancellationToken);
    }
}


