using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddValidationRuleCommandHandler : ICommandHandler<AddValidationRuleCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddValidationRuleCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddValidationRuleCommand command, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.ValidationSpecs
            .FirstOrDefaultAsync(s => s.Id == command.ValidationSpecId, cancellationToken);

        if (spec == null)
            throw new InvalidOperationException($"Validation spec {command.ValidationSpecId} not found");

        if (spec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft validation specs can be modified. Current status: {spec.Status}");

        var entity = new ValidationRuleEntity
        {
            Id = Guid.NewGuid(),
            ValidationSpecId = command.ValidationSpecId,
            RuleType = command.RuleType,
            Severity = command.Severity,
            Parameters = command.Parameters
        };

        _dbContext.ValidationRules.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}


