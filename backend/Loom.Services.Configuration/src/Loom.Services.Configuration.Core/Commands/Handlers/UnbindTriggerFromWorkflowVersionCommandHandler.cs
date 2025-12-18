using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class UnbindTriggerFromWorkflowVersionCommandHandler : ICommandHandler<UnbindTriggerFromWorkflowVersionCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public UnbindTriggerFromWorkflowVersionCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UnbindTriggerFromWorkflowVersionCommand command, CancellationToken cancellationToken = default)
    {
        var binding = await _dbContext.TriggerBindings
            .FirstOrDefaultAsync(b => b.Id == command.TriggerBindingId, cancellationToken);

        if (binding == null)
            throw new InvalidOperationException($"Trigger binding {command.TriggerBindingId} not found");

        _dbContext.TriggerBindings.Remove(binding);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


