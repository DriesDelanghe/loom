namespace Loom.Services.MasterDataConfiguration.Domain.Validation;

public class ValidationRule
{
    public Guid Id { get; set; }
    public Guid ValidationSpecId { get; set; }
    public RuleType RuleType { get; set; }
    public Severity Severity { get; set; }
    public string Parameters { get; set; } = default!; // JSONB
}

