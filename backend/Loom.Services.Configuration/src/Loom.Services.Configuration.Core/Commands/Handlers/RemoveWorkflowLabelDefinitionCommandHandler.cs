using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class RemoveWorkflowLabelDefinitionCommandHandler : ICommandHandler<RemoveWorkflowLabelDefinitionCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public RemoveWorkflowLabelDefinitionCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveWorkflowLabelDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var label = await _dbContext.Labels
            .Include(l => l.WorkflowVersion)
            .FirstOrDefaultAsync(l => l.Id == command.LabelId, cancellationToken);

        if (label == null)
            throw new InvalidOperationException($"Label {command.LabelId} not found");

        if (label.WorkflowVersion.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Labels can only be removed from draft versions");

        _dbContext.Labels.Remove(label);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


