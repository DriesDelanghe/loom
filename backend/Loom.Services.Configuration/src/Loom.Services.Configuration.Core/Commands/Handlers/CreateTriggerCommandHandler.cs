using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class CreateTriggerCommandHandler : ICommandHandler<CreateTriggerCommand, Guid>
{
    private readonly ConfigurationDbContext _dbContext;

    public CreateTriggerCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(CreateTriggerCommand command, CancellationToken cancellationToken = default)
    {
        var trigger = new TriggerEntity
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            Type = command.Type,
            ConfigJson = command.Config?.RootElement.GetRawText(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Triggers.Add(trigger);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return trigger.Id;
    }
}


