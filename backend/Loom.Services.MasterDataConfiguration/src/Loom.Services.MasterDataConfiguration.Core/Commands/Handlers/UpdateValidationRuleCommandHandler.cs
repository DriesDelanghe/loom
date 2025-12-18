using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class UpdateValidationRuleCommandHandler : ICommandHandler<UpdateValidationRuleCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public UpdateValidationRuleCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UpdateValidationRuleCommand command, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.ValidationRules
            .Include(r => r.ValidationSpec)
            .FirstOrDefaultAsync(r => r.Id == command.RuleId, cancellationToken);

        if (rule == null)
            throw new InvalidOperationException($"Validation rule {command.RuleId} not found");

        if (rule.ValidationSpec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft validation specs can be modified. Current status: {rule.ValidationSpec.Status}");

        // Update only provided fields
        if (command.RuleType.HasValue)
            rule.RuleType = command.RuleType.Value;
        if (command.Severity.HasValue)
            rule.Severity = command.Severity.Value;
        if (command.Parameters != null)
            rule.Parameters = command.Parameters;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


