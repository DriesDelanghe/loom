using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Core.Services;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class PublishTransformationSpecCommandHandler : ICommandHandler<PublishTransformationSpecCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;
    private readonly IStaticValidationEngine _validationEngine;

    public PublishTransformationSpecCommandHandler(
        MasterDataConfigurationDbContext dbContext,
        IStaticValidationEngine validationEngine)
    {
        _dbContext = dbContext;
        _validationEngine = validationEngine;
    }

    public async Task<bool> HandleAsync(PublishTransformationSpecCommand command, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.TransformationSpecs
            .FirstOrDefaultAsync(s => s.Id == command.TransformationSpecId, cancellationToken);

        if (spec == null)
            throw new InvalidOperationException($"Transformation spec {command.TransformationSpecId} not found");

        if (spec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft transformation specs can be published. Current status: {spec.Status}");

        var validationResult = await _validationEngine.ValidateTransformationSpecAsync(command.TransformationSpecId, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessages = string.Join(", ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
            throw new InvalidOperationException($"Transformation spec validation failed: {errorMessages}");
        }

        var previousPublished = await _dbContext.TransformationSpecs
            .Where(s => s.SourceSchemaId == spec.SourceSchemaId && s.TargetSchemaId == spec.TargetSchemaId && s.Status == SchemaStatus.Published)
            .ToListAsync(cancellationToken);

        foreach (var prev in previousPublished)
        {
            prev.Status = SchemaStatus.Archived;
        }

        spec.Status = SchemaStatus.Published;
        spec.PublishedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


