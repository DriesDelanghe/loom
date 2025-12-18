using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Core.Services;

namespace Loom.Services.Configuration.Core.Queries.Handlers;

public class ValidateWorkflowVersionQueryHandler : IQueryHandler<ValidateWorkflowVersionQuery, ValidationResultDto>
{
    private readonly IWorkflowValidator _validator;

    public ValidateWorkflowVersionQueryHandler(IWorkflowValidator validator)
    {
        _validator = validator;
    }

    public async Task<ValidationResultDto> HandleAsync(ValidateWorkflowVersionQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _validator.ValidateAsync(query.WorkflowVersionId, cancellationToken);
        return new ValidationResultDto(
            result.IsValid,
            result.Errors,
            result.Warnings
        );
    }
}


