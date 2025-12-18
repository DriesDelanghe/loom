using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class RemoveSchemaTagByValueCommandHandler : ICommandHandler<RemoveSchemaTagByValueCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public RemoveSchemaTagByValueCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveSchemaTagByValueCommand command, CancellationToken cancellationToken = default)
    {
        var tag = await _dbContext.SchemaTags
            .FirstOrDefaultAsync(t => t.DataSchemaId == command.SchemaId && t.Tag == command.Tag, cancellationToken);

        if (tag == null)
            throw new InvalidOperationException($"Tag '{command.Tag}' not found for schema {command.SchemaId}");

        _dbContext.SchemaTags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


