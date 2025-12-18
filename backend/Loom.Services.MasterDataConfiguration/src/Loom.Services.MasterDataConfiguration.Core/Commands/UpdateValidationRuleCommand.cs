using Loom.Services.MasterDataConfiguration.Domain.Validation;

namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record UpdateValidationRuleCommand(
    Guid RuleId,
    RuleType? RuleType,
    Severity? Severity,
    string? Parameters
);


