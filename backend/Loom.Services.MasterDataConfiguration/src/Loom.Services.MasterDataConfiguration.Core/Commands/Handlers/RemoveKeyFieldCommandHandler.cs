using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class RemoveKeyFieldCommandHandler : ICommandHandler<RemoveKeyFieldCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public RemoveKeyFieldCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveKeyFieldCommand command, CancellationToken cancellationToken = default)
    {
        var keyField = await _dbContext.KeyFields
            .Include(f => f.KeyDefinition)
            .ThenInclude(k => k.DataSchema)
            .FirstOrDefaultAsync(f => f.Id == command.KeyFieldId, cancellationToken);

        if (keyField == null)
            throw new InvalidOperationException($"Key field {command.KeyFieldId} not found");

        if (keyField.KeyDefinition.DataSchema.Status != Domain.Schemas.SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft schemas can be modified. Current status: {keyField.KeyDefinition.DataSchema.Status}");

        _dbContext.KeyFields.Remove(keyField);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

