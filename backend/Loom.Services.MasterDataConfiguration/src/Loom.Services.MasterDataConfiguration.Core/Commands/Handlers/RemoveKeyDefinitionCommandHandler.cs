using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class RemoveKeyDefinitionCommandHandler : ICommandHandler<RemoveKeyDefinitionCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public RemoveKeyDefinitionCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveKeyDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var keyDefinition = await _dbContext.KeyDefinitions
            .Include(k => k.DataSchema)
            .FirstOrDefaultAsync(k => k.Id == command.KeyDefinitionId, cancellationToken);

        if (keyDefinition == null)
            throw new InvalidOperationException($"Key definition {command.KeyDefinitionId} not found");

        if (keyDefinition.DataSchema.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft schemas can be modified. Current status: {keyDefinition.DataSchema.Status}");

        _dbContext.KeyDefinitions.Remove(keyDefinition);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

