using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class DeleteWorkflowVersionCommandHandler : ICommandHandler<DeleteWorkflowVersionCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public DeleteWorkflowVersionCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(DeleteWorkflowVersionCommand command, CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.WorkflowVersions
            .Include(v => v.TriggerBindings)
                .ThenInclude(tb => tb.NodeBindings)
            .FirstOrDefaultAsync(v => v.Id == command.WorkflowVersionId, cancellationToken);

        if (version == null)
            throw new InvalidOperationException($"Workflow version {command.WorkflowVersionId} not found");

        if (version.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException($"Only draft workflow versions can be deleted. Current status: {version.Status}");

        // Delete TriggerNodeBindings first to avoid foreign key constraint violation
        // (Nodes have RESTRICT constraint from TriggerNodeBindings)
        foreach (var triggerBinding in version.TriggerBindings)
        {
            _dbContext.TriggerNodeBindings.RemoveRange(triggerBinding.NodeBindings);
        }

        // Delete TriggerBindings (will be cascade deleted, but explicit for clarity)
        _dbContext.TriggerBindings.RemoveRange(version.TriggerBindings);

        // Now we can safely delete the version (which will cascade delete nodes, connections, etc.)
        _dbContext.WorkflowVersions.Remove(version);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

