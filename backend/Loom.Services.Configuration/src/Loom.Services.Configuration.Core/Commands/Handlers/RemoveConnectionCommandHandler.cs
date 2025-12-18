using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class RemoveConnectionCommandHandler : ICommandHandler<RemoveConnectionCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public RemoveConnectionCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveConnectionCommand command, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.Connections
            .Include(c => c.WorkflowVersion)
            .FirstOrDefaultAsync(c => c.Id == command.ConnectionId, cancellationToken);

        if (connection == null)
            throw new InvalidOperationException($"Connection {command.ConnectionId} not found");

        if (connection.WorkflowVersion.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Connections can only be removed from draft versions");

        _dbContext.Connections.Remove(connection);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


