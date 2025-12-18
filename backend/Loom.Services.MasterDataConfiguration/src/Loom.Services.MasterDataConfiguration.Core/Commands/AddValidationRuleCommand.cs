using Loom.Services.MasterDataConfiguration.Domain.Validation;

namespace Loom.Services.MasterDataConfiguration.Core.Commands;

public record AddValidationRuleCommand(
    Guid ValidationSpecId,
    RuleType RuleType,
    Severity Severity,
    string Parameters
);


