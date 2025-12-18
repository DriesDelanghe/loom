using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class GetValidationSpecDetailsQueryHandler : IQueryHandler<GetValidationSpecDetailsQuery, ValidationSpecDetails?>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public GetValidationSpecDetailsQueryHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ValidationSpecDetails?> HandleAsync(GetValidationSpecDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.ValidationSpecs
            .Include(s => s.Rules)
            .Include(s => s.References)
            .FirstOrDefaultAsync(s => s.Id == query.ValidationSpecId, cancellationToken);

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

public class ValidationSpecDetails
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid DataSchemaId { get; set; }
    public int Version { get; set; }
    public Domain.Schemas.SchemaStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public IReadOnlyList<ValidationRuleSummary> Rules { get; set; } = Array.Empty<ValidationRuleSummary>();
    public IReadOnlyList<ValidationReferenceSummary> References { get; set; } = Array.Empty<ValidationReferenceSummary>();
}

public class ValidationRuleSummary
{
    public Guid Id { get; set; }
    public Domain.Validation.RuleType RuleType { get; set; }
    public Domain.Validation.Severity Severity { get; set; }
    public string Parameters { get; set; } = default!;
}

public class ValidationReferenceSummary
{
    public Guid Id { get; set; }
    public string FieldPath { get; set; } = default!;
    public Guid ChildValidationSpecId { get; set; }
}


