using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class UpdateNodeConfigCommandHandler : ICommandHandler<UpdateNodeConfigCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public UpdateNodeConfigCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UpdateNodeConfigCommand command, CancellationToken cancellationToken = default)
    {
        var node = await _dbContext.Nodes
            .Include(n => n.WorkflowVersion)
            .FirstOrDefaultAsync(n => n.Id == command.NodeId, cancellationToken);

        if (node == null)
            throw new InvalidOperationException($"Node {command.NodeId} not found");

        if (node.WorkflowVersion.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Node config can only be updated in draft versions");

        node.ConfigJson = command.Config?.RootElement.GetRawText();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


