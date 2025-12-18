using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class AddSchemaTagCommandHandler : ICommandHandler<AddSchemaTagCommand, Guid>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public AddSchemaTagCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddSchemaTagCommand command, CancellationToken cancellationToken = default)
    {
        var schema = await _dbContext.DataSchemas
            .FirstOrDefaultAsync(s => s.Id == command.SchemaId, cancellationToken);

        if (schema == null)
            throw new InvalidOperationException($"Schema {command.SchemaId} not found");

        // Check if tag already exists
        var existingTag = await _dbContext.SchemaTags
            .FirstOrDefaultAsync(t => t.DataSchemaId == command.SchemaId && t.Tag == command.Tag, cancellationToken);

        if (existingTag != null)
            throw new InvalidOperationException($"Tag '{command.Tag}' already exists for schema {command.SchemaId}");

        var tagEntity = new SchemaTagEntity
        {
            Id = Guid.NewGuid(),
            DataSchemaId = command.SchemaId,
            Tag = command.Tag.Trim()
        };

        _dbContext.SchemaTags.Add(tagEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return tagEntity.Id;
    }
}


