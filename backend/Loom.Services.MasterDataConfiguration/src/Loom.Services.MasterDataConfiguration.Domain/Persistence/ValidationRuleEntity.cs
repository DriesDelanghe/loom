using Loom.Services.MasterDataConfiguration.Domain.Validation;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class ValidationRuleEntity
{
    public Guid Id { get; set; }
    public Guid ValidationSpecId { get; set; }
    public RuleType RuleType { get; set; }
    public Severity Severity { get; set; }
    public string Parameters { get; set; } = default!; // JSONB

    public ValidationSpecEntity ValidationSpec { get; set; } = null!;

    public ValidationRule ToDomain()
    {
        return new ValidationRule
        {
            Id = Id,
            ValidationSpecId = ValidationSpecId,
            RuleType = RuleType,
            Severity = Severity,
            Parameters = Parameters
        };
    }

    public static ValidationRuleEntity FromDomain(ValidationRule rule)
    {
        return new ValidationRuleEntity
        {
            Id = rule.Id,
            ValidationSpecId = rule.ValidationSpecId,
            RuleType = rule.RuleType,
            Severity = rule.Severity,
            Parameters = rule.Parameters
        };
    }
}
