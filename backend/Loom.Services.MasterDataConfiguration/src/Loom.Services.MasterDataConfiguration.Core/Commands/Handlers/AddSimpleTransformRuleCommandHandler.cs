using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddSimpleTransformRuleCommandHandler : ICommandHandler<AddSimpleTransformRuleCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddSimpleTransformRuleCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddSimpleTransformRuleCommand command, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.TransformationSpecs
            .FirstOrDefaultAsync(s => s.Id == command.TransformationSpecId, cancellationToken);

        if (spec == null)
            throw new InvalidOperationException($"Transformation spec {command.TransformationSpecId} not found");

        if (spec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft transformation specs can be modified. Current status: {spec.Status}");

        if (spec.Mode != Domain.Transformation.TransformationMode.Simple)
            throw new InvalidOperationException("Can only add simple transform rules to Simple mode transformation specs");

        var entity = new SimpleTransformRuleEntity
        {
            Id = Guid.NewGuid(),
            TransformationSpecId = command.TransformationSpecId,
            SourcePath = command.SourcePath,
            TargetPath = command.TargetPath,
            ConverterId = command.ConverterId,
            Required = command.Required,
            Order = command.Order
        };

        _dbContext.SimpleTransformRules.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}


