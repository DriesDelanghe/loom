using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class UpdateSimpleTransformRuleCommandHandler : ICommandHandler<UpdateSimpleTransformRuleCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public UpdateSimpleTransformRuleCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UpdateSimpleTransformRuleCommand command, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.SimpleTransformRules
            .Include(r => r.TransformationSpec)
            .FirstOrDefaultAsync(r => r.Id == command.RuleId, cancellationToken);

        if (rule == null)
            throw new InvalidOperationException($"Simple transform rule {command.RuleId} not found");

        if (rule.TransformationSpec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft transformation specs can be modified. Current status: {rule.TransformationSpec.Status}");

        // Update only provided fields
        if (command.SourcePath != null)
            rule.SourcePath = command.SourcePath;
        if (command.TargetPath != null)
            rule.TargetPath = command.TargetPath;
        if (command.ConverterId.HasValue)
            rule.ConverterId = command.ConverterId;
        if (command.Required.HasValue)
            rule.Required = command.Required.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

