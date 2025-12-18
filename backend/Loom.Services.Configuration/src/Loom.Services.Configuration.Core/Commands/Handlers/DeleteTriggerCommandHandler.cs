using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class DeleteTriggerCommandHandler : ICommandHandler<DeleteTriggerCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public DeleteTriggerCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(DeleteTriggerCommand command, CancellationToken cancellationToken = default)
    {
        var trigger = await _dbContext.Triggers
            .Include(t => t.Bindings)
            .FirstOrDefaultAsync(t => t.Id == command.TriggerId, cancellationToken);

        if (trigger == null)
            throw new InvalidOperationException($"Trigger {command.TriggerId} not found");

        if (trigger.Bindings.Any())
            throw new InvalidOperationException("Cannot delete trigger with active bindings. Unbind all workflows first.");

        _dbContext.Triggers.Remove(trigger);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


