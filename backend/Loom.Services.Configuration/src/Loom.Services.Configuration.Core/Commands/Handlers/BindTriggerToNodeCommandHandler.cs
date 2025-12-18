using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class BindTriggerToNodeCommandHandler : ICommandHandler<BindTriggerToNodeCommand, Guid>
{
    private readonly ConfigurationDbContext _dbContext;

    public BindTriggerToNodeCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(BindTriggerToNodeCommand command, CancellationToken cancellationToken = default)
    {
        var triggerBinding = await _dbContext.TriggerBindings
            .Include(tb => tb.WorkflowVersion)
            .Include(tb => tb.NodeBindings)
            .FirstOrDefaultAsync(tb => tb.Id == command.TriggerBindingId, cancellationToken);

        if (triggerBinding == null)
            throw new InvalidOperationException($"Trigger binding {command.TriggerBindingId} not found");

        if (triggerBinding.WorkflowVersion.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Can only bind triggers to nodes in draft workflow versions");

        var node = await _dbContext.Nodes
            .FirstOrDefaultAsync(n => n.Id == command.EntryNodeId, cancellationToken);

        if (node == null)
            throw new InvalidOperationException($"Node {command.EntryNodeId} not found");

        if (node.WorkflowVersionId != triggerBinding.WorkflowVersionId)
            throw new InvalidOperationException("Entry node must belong to the same workflow version as the trigger binding");

        var existingBinding = triggerBinding.NodeBindings
            .FirstOrDefault(nb => nb.EntryNodeId == command.EntryNodeId);

        if (existingBinding != null)
            throw new InvalidOperationException("This node is already bound as an entry point for this trigger");

        var maxOrder = triggerBinding.NodeBindings.Any() 
            ? triggerBinding.NodeBindings.Max(nb => nb.Order) 
            : -1;

        var nodeBinding = new TriggerNodeBindingEntity
        {
            Id = Guid.NewGuid(),
            TriggerBindingId = command.TriggerBindingId,
            EntryNodeId = command.EntryNodeId,
            Order = command.Order ?? (maxOrder + 1)
        };

        _dbContext.TriggerNodeBindings.Add(nodeBinding);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return nodeBinding.Id;
    }
}

