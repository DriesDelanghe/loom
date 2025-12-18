using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class UpdateTriggerConfigCommandHandler : ICommandHandler<UpdateTriggerConfigCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public UpdateTriggerConfigCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UpdateTriggerConfigCommand command, CancellationToken cancellationToken = default)
    {
        var trigger = await _dbContext.Triggers
            .FirstOrDefaultAsync(t => t.Id == command.TriggerId, cancellationToken);

        if (trigger == null)
            throw new InvalidOperationException($"Trigger {command.TriggerId} not found");

        trigger.ConfigJson = command.Config?.RootElement.GetRawText();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


