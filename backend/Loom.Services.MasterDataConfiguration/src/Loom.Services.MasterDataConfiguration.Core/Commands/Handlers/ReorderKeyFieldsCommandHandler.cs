using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class ReorderKeyFieldsCommandHandler : ICommandHandler<ReorderKeyFieldsCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public ReorderKeyFieldsCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(ReorderKeyFieldsCommand command, CancellationToken cancellationToken = default)
    {
        var keyDefinition = await _dbContext.KeyDefinitions
            .Include(k => k.DataSchema)
            .Include(k => k.KeyFields)
            .FirstOrDefaultAsync(k => k.Id == command.KeyDefinitionId, cancellationToken);

        if (keyDefinition == null)
            throw new InvalidOperationException($"Key definition {command.KeyDefinitionId} not found");

        if (keyDefinition.DataSchema.Status != Domain.Schemas.SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft schemas can be modified. Current status: {keyDefinition.DataSchema.Status}");

        // Verify all field IDs belong to this key definition
        var fieldIds = keyDefinition.KeyFields.Select(f => f.Id).ToHashSet();
        foreach (var fieldId in command.KeyFieldIdsInOrder)
        {
            if (!fieldIds.Contains(fieldId))
                throw new InvalidOperationException($"Key field {fieldId} does not belong to key definition {command.KeyDefinitionId}");
        }

        // Verify all fields are included
        if (fieldIds.Count != command.KeyFieldIdsInOrder.Count)
            throw new InvalidOperationException("All key fields must be included in the reorder operation");

        // Update order
        for (int i = 0; i < command.KeyFieldIdsInOrder.Count; i++)
        {
            var field = keyDefinition.KeyFields.First(f => f.Id == command.KeyFieldIdsInOrder[i]);
            field.Order = i;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


