using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;

public class RemoveSchemaTagCommandHandler : ICommandHandler<RemoveSchemaTagCommand, bool>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public RemoveSchemaTagCommandHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveSchemaTagCommand command, CancellationToken cancellationToken = default)
    {
        var tag = await _dbContext.SchemaTags
            .FirstOrDefaultAsync(t => t.Id == command.TagId, cancellationToken);

        if (tag == null)
            throw new InvalidOperationException($"Tag {command.TagId} not found");

        _dbContext.SchemaTags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

