using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class GetValidationSpecBySchemaIdQueryHandler : IQueryHandler<GetValidationSpecBySchemaIdQuery, ValidationSpecDetails?>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public GetValidationSpecBySchemaIdQueryHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ValidationSpecDetails?> HandleAsync(GetValidationSpecBySchemaIdQuery query, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.ValidationSpecs
            .Include(s => s.Rules)
            .Include(s => s.References)
            .Where(s => s.DataSchemaId == query.DataSchemaId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (spec == null)
            return null;

        return new ValidationSpecDetails
        {
            Id = spec.Id,
            TenantId = spec.TenantId,
            DataSchemaId = spec.DataSchemaId,
            Version = spec.Version,
            Status = spec.Status,
            Description = spec.Description,
            CreatedAt = spec.CreatedAt,
            PublishedAt = spec.PublishedAt,
            Rules = spec.Rules.Select(r => new ValidationRuleSummary
            {
                Id = r.Id,
                RuleType = r.RuleType,
                Severity = r.Severity,
                Parameters = r.Parameters
            }).ToList(),
            References = spec.References.Select(r => new ValidationReferenceSummary
            {
                Id = r.Id,
                FieldPath = r.FieldPath,
                ChildValidationSpecId = r.ChildValidationSpecId
            }).ToList()
        };
    }
}

