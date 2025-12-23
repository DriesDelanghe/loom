using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class UpdateFieldDefinitionCommandHandler : ICommandHandler<UpdateFieldDefinitionCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public UpdateFieldDefinitionCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UpdateFieldDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var field = await _dbContext.FieldDefinitions
            .Include(f => f.DataSchema)
            .FirstOrDefaultAsync(f => f.Id == command.FieldDefinitionId, cancellationToken);

        if (field == null)
            throw new InvalidOperationException($"Field definition {command.FieldDefinitionId} not found");

        if (field.DataSchema.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft schemas can be modified. Current status: {field.DataSchema.Status}");

        // Update only provided fields
        if (command.Path != null)
            field.Path = command.Path;
        if (command.FieldType.HasValue)
            field.FieldType = command.FieldType.Value;
        if (command.ScalarType.HasValue)
            field.ScalarType = command.ScalarType;
        if (command.ElementSchemaId.HasValue)
            field.ElementSchemaId = command.ElementSchemaId;
        if (command.Required.HasValue)
            field.Required = command.Required.Value;
        if (command.Description != null)
            field.Description = command.Description;

        // Validate field type constraints after update
        if (field.FieldType == FieldType.Scalar)
        {
            if (!field.ScalarType.HasValue)
                throw new InvalidOperationException("Scalar fields must have a ScalarType");
            if (field.ElementSchemaId.HasValue)
                throw new InvalidOperationException("Scalar fields cannot have an ElementSchemaId");
        }
        else if (field.FieldType == FieldType.Object)
        {
            if (!field.ElementSchemaId.HasValue)
                throw new InvalidOperationException("Object fields must have an ElementSchemaId");
            if (field.ScalarType.HasValue)
                throw new InvalidOperationException("Object fields cannot have a ScalarType");
        }
        else if (field.FieldType == FieldType.Array)
        {
            var hasScalarType = field.ScalarType.HasValue;
            var hasElementSchemaId = field.ElementSchemaId.HasValue;

            if (!hasScalarType && !hasElementSchemaId)
                throw new InvalidOperationException("Array field must define an element type (either ScalarType for scalar arrays or ElementSchemaId for object arrays)");
            if (hasScalarType && hasElementSchemaId)
                throw new InvalidOperationException("Array field cannot define both scalar and object element types");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


