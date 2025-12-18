using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Core.Services;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class PublishValidationSpecCommandHandler : ICommandHandler<PublishValidationSpecCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;
    private readonly IStaticValidationEngine _validationEngine;

    public PublishValidationSpecCommandHandler(
        MasterDataConfigurationDbContext dbContext,
        IStaticValidationEngine validationEngine)
    {
        _dbContext = dbContext;
        _validationEngine = validationEngine;
    }

    public async Task<bool> HandleAsync(PublishValidationSpecCommand command, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.ValidationSpecs
            .FirstOrDefaultAsync(s => s.Id == command.ValidationSpecId, cancellationToken);

        if (spec == null)
            throw new InvalidOperationException($"Validation spec {command.ValidationSpecId} not found");

        if (spec.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft validation specs can be published. Current status: {spec.Status}");

        var validationResult = await _validationEngine.ValidateValidationSpecAsync(command.ValidationSpecId, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorMessages = string.Join(", ", validationResult.Errors.Select(e => $"{e.Field}: {e.Message}"));
            throw new InvalidOperationException($"Validation spec validation failed: {errorMessages}");
        }

        var previousPublished = await _dbContext.ValidationSpecs
            .Where(s => s.DataSchemaId == spec.DataSchemaId && s.Status == SchemaStatus.Published)
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

