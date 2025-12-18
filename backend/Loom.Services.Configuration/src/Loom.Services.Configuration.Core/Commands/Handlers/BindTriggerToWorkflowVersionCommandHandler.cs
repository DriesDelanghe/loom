using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class BindTriggerToWorkflowVersionCommandHandler : ICommandHandler<BindTriggerToWorkflowVersionCommand, Guid>
{
    private readonly ConfigurationDbContext _dbContext;

    public BindTriggerToWorkflowVersionCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(BindTriggerToWorkflowVersionCommand command, CancellationToken cancellationToken = default)
    {
        var trigger = await _dbContext.Triggers.FindAsync(new object[] { command.TriggerId }, cancellationToken);
        if (trigger == null)
            throw new InvalidOperationException($"Trigger {command.TriggerId} not found");

        var version = await _dbContext.WorkflowVersions.FindAsync(new object[] { command.WorkflowVersionId }, cancellationToken);
        if (version == null)
            throw new InvalidOperationException($"Workflow version {command.WorkflowVersionId} not found");

        var existingBinding = await _dbContext.TriggerBindings
            .FirstOrDefaultAsync(b => b.TriggerId == command.TriggerId && b.WorkflowVersionId == command.WorkflowVersionId, cancellationToken);

        if (existingBinding != null)
            throw new InvalidOperationException("Trigger is already bound to this workflow version");

        var binding = new TriggerBindingEntity
        {
            Id = Guid.NewGuid(),
            TriggerId = command.TriggerId,
            WorkflowVersionId = command.WorkflowVersionId,
            Enabled = command.Enabled,
            Priority = command.Priority
        };

        _dbContext.TriggerBindings.Add(binding);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return binding.Id;
    }
}


