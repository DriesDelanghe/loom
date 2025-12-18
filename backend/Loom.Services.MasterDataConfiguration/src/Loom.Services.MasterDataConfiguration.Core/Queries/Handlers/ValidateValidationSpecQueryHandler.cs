using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Core.Services;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class ValidateValidationSpecQueryHandler : IQueryHandler<ValidateValidationSpecQuery, ValidationResult>
{
    private readonly IStaticValidationEngine _validationEngine;

    public ValidateValidationSpecQueryHandler(IStaticValidationEngine validationEngine)
    {
        _validationEngine = validationEngine;
    }

    public async Task<ValidationResult> HandleAsync(ValidateValidationSpecQuery query, CancellationToken cancellationToken = default)
    {
        return await _validationEngine.ValidateValidationSpecAsync(query.ValidationSpecId, cancellationToken);
    }
}


