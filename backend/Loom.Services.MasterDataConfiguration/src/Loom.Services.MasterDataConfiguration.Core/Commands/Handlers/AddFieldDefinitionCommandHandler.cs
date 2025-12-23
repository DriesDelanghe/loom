using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddFieldDefinitionCommandHandler : ICommandHandler<AddFieldDefinitionCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddFieldDefinitionCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddFieldDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var schema = await _dbContext.DataSchemas
            .FirstOrDefaultAsync(s => s.Id == command.DataSchemaId, cancellationToken);

        if (schema == null)
            throw new InvalidOperationException($"Data schema {command.DataSchemaId} not found");

        if (schema.Status != SchemaStatus.Draft)
            throw new InvalidOperationException($"Only draft schemas can be modified. Current status: {schema.Status}");

        var existing = await _dbContext.FieldDefinitions
            .FirstOrDefaultAsync(f => f.DataSchemaId == command.DataSchemaId && f.Path == command.Path, cancellationToken);

        if (existing != null)
            throw new InvalidOperationException($"Field with path '{command.Path}' already exists in schema {command.DataSchemaId}");

        // Validate field type constraints
        if (command.FieldType == FieldType.Scalar)
        {
            if (!command.ScalarType.HasValue)
                throw new InvalidOperationException("Scalar fields must have a ScalarType");
            if (command.ElementSchemaId.HasValue)
                throw new InvalidOperationException("Scalar fields cannot have an ElementSchemaId");
        }
        else if (command.FieldType == FieldType.Object)
        {
            if (!command.ElementSchemaId.HasValue)
                throw new InvalidOperationException("Object fields must have an ElementSchemaId");
            if (command.ScalarType.HasValue)
                throw new InvalidOperationException("Object fields cannot have a ScalarType");
        }
        else if (command.FieldType == FieldType.Array)
        {
            var hasScalarType = command.ScalarType.HasValue;
            var hasElementSchemaId = command.ElementSchemaId.HasValue;

            if (!hasScalarType && !hasElementSchemaId)
                throw new InvalidOperationException("Array field must define an element type (either ScalarType for scalar arrays or ElementSchemaId for object arrays)");
            if (hasScalarType && hasElementSchemaId)
                throw new InvalidOperationException("Array field cannot define both scalar and object element types");
        }

        if (command.ElementSchemaId.HasValue)
        {
            var elementSchema = await _dbContext.DataSchemas
                .FirstOrDefaultAsync(s => s.Id == command.ElementSchemaId.Value, cancellationToken);

            if (elementSchema == null)
                throw new InvalidOperationException($"Element schema {command.ElementSchemaId.Value} not found");

            // Allow Draft and Published schemas to be referenced when editing
            // Published requirement is enforced during publish validation
            if (elementSchema.Status == SchemaStatus.Archived)
                throw new InvalidOperationException($"Referenced schema {command.ElementSchemaId.Value} cannot be Archived");

            if (elementSchema.Role != schema.Role)
                throw new InvalidOperationException($"Referenced schema {command.ElementSchemaId.Value} must have the same Role as the parent schema");
        }

        var entity = new FieldDefinitionEntity
        {
            Id = Guid.NewGuid(),
            DataSchemaId = command.DataSchemaId,
            Path = command.Path,
            FieldType = command.FieldType,
            ScalarType = command.ScalarType,
            ElementSchemaId = command.ElementSchemaId,
            Required = command.Required,
            Description = command.Description
        };

        _dbContext.FieldDefinitions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

