using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class UnbindTriggerFromNodeCommandHandler : ICommandHandler<UnbindTriggerFromNodeCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public UnbindTriggerFromNodeCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UnbindTriggerFromNodeCommand command, CancellationToken cancellationToken = default)
    {
        var nodeBinding = await _dbContext.TriggerNodeBindings
            .Include(nb => nb.TriggerBinding)
                .ThenInclude(tb => tb.WorkflowVersion)
            .FirstOrDefaultAsync(nb => nb.Id == command.TriggerNodeBindingId, cancellationToken);

        if (nodeBinding == null)
            throw new InvalidOperationException($"Trigger node binding {command.TriggerNodeBindingId} not found");

        if (nodeBinding.TriggerBinding.WorkflowVersion.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Can only unbind triggers from nodes in draft workflow versions");

        _dbContext.TriggerNodeBindings.Remove(nodeBinding);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

