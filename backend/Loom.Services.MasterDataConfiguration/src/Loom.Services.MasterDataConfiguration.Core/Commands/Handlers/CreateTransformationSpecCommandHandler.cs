using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class CreateTransformationSpecCommandHandler : ICommandHandler<CreateTransformationSpecCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public CreateTransformationSpecCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(CreateTransformationSpecCommand command, CancellationToken cancellationToken = default)
    {
        var sourceSchema = await _dbContext.DataSchemas
            .FirstOrDefaultAsync(s => s.Id == command.SourceSchemaId, cancellationToken);

        if (sourceSchema == null)
            throw new InvalidOperationException($"Source schema {command.SourceSchemaId} not found");

        var targetSchema = await _dbContext.DataSchemas
            .FirstOrDefaultAsync(s => s.Id == command.TargetSchemaId, cancellationToken);

        if (targetSchema == null)
            throw new InvalidOperationException($"Target schema {command.TargetSchemaId} not found");

        var latestVersion = await _dbContext.TransformationSpecs
            .Where(s => s.SourceSchemaId == command.SourceSchemaId && s.TargetSchemaId == command.TargetSchemaId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);

        var version = latestVersion == null ? 1 : latestVersion.Version + 1;

        var entity = new TransformationSpecEntity
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            SourceSchemaId = command.SourceSchemaId,
            TargetSchemaId = command.TargetSchemaId,
            Mode = command.Mode,
            Cardinality = command.Cardinality,
            Version = version,
            Status = SchemaStatus.Draft,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.TransformationSpecs.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}


