using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Core.Services;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class ValidateTransformationSpecQueryHandler : IQueryHandler<ValidateTransformationSpecQuery, ValidationResult>
{
    private readonly IStaticValidationEngine _validationEngine;

    public ValidateTransformationSpecQueryHandler(IStaticValidationEngine validationEngine)
    {
        _validationEngine = validationEngine;
    }

    public async Task<ValidationResult> HandleAsync(ValidateTransformationSpecQuery query, CancellationToken cancellationToken = default)
    {
        return await _validationEngine.ValidateTransformationSpecAsync(query.TransformationSpecId, cancellationToken);
    }
}


