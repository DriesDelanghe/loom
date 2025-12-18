using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class RemoveValidationRuleCommandHandler : ICommandHandler<RemoveValidationRuleCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public RemoveValidationRuleCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveValidationRuleCommand command, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.ValidationRules
            .Include(r => r.ValidationSpec)
            .FirstOrDefaultAsync(r => r.Id == command.RuleId, cancellationToken);

        if (rule == null)
            throw new InvalidOperationException($"Validation rule {command.RuleId} not found");

        if (rule.ValidationSpec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft validation specs can be modified. Current status: {rule.ValidationSpec.Status}");

        _dbContext.ValidationRules.Remove(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

