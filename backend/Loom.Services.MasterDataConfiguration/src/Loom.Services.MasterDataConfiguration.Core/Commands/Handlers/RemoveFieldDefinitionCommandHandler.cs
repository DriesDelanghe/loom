using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class RemoveFieldDefinitionCommandHandler : ICommandHandler<RemoveFieldDefinitionCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public RemoveFieldDefinitionCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveFieldDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var field = await _dbContext.FieldDefinitions
            .Include(f => f.DataSchema)
            .FirstOrDefaultAsync(f => f.Id == command.FieldDefinitionId, cancellationToken);

        if (field == null)
            throw new InvalidOperationException($"Field definition {command.FieldDefinitionId} not found");

        if (field.DataSchema.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft schemas can be modified. Current status: {field.DataSchema.Status}");

        _dbContext.FieldDefinitions.Remove(field);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


