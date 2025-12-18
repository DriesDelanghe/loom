using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Domain.Workflows;
using Loom.Services.Configuration.Core.Services;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class PublishWorkflowVersionCommandHandler : ICommandHandler<PublishWorkflowVersionCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;
    private readonly IWorkflowValidator _validator;

    public PublishWorkflowVersionCommandHandler(
        ConfigurationDbContext dbContext,
        IWorkflowValidator validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<bool> HandleAsync(PublishWorkflowVersionCommand command, CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.WorkflowVersions
            .Include(v => v.Definition)
            .Include(v => v.Nodes)
            .Include(v => v.Connections)
            .Include(v => v.Variables)
            .Include(v => v.Labels)
            .Include(v => v.TriggerBindings)
                .ThenInclude(tb => tb.NodeBindings)
            .FirstOrDefaultAsync(v => v.Id == command.WorkflowVersionId, cancellationToken);

        if (version == null)
            throw new InvalidOperationException($"Workflow version {command.WorkflowVersionId} not found");

        if (version.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException($"Only draft versions can be published. Current status: {version.Status}");

        ValidateTriggerBindings(version);

        var validationResult = await _validator.ValidateAsync(version.Id, cancellationToken);
        if (!validationResult.IsValid)
            throw new InvalidOperationException($"Workflow validation failed: {string.Join(", ", validationResult.Errors)}");

        var previousPublished = await _dbContext.WorkflowVersions
            .Where(v => v.DefinitionId == version.DefinitionId && v.Status == WorkflowStatus.Published)
            .ToListAsync(cancellationToken);

        foreach (var prev in previousPublished)
        {
            prev.Status = WorkflowStatus.Archived;
        }

        version.Status = WorkflowStatus.Published;
        version.PublishedAt = DateTime.UtcNow;
        version.PublishedBy = command.PublishedBy;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateTriggerBindings(Domain.Persistence.WorkflowVersionEntity version)
    {
        if (!version.TriggerBindings.Any())
            throw new InvalidOperationException("Workflow version must have at least one trigger binding to be published");

        var nodeIds = version.Nodes.Select(n => n.Id).ToHashSet();

        foreach (var triggerBinding in version.TriggerBindings)
        {
            if (!triggerBinding.NodeBindings.Any())
                throw new InvalidOperationException($"Trigger binding {triggerBinding.Id} must have at least one entry node");

            foreach (var nodeBinding in triggerBinding.NodeBindings)
            {
                if (!nodeIds.Contains(nodeBinding.EntryNodeId))
                    throw new InvalidOperationException($"Entry node {nodeBinding.EntryNodeId} does not belong to workflow version");
            }
        }
    }
}
