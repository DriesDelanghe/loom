using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class AddWorkflowLabelDefinitionCommandHandler : ICommandHandler<AddWorkflowLabelDefinitionCommand, Guid>
{
    private readonly ConfigurationDbContext _dbContext;

    public AddWorkflowLabelDefinitionCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddWorkflowLabelDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.WorkflowVersions
            .FirstOrDefaultAsync(v => v.Id == command.WorkflowVersionId, cancellationToken);

        if (version == null)
            throw new InvalidOperationException($"Workflow version {command.WorkflowVersionId} not found");

        if (version.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Labels can only be added to draft versions");

        var keyExists = await _dbContext.Labels
            .AnyAsync(l => l.WorkflowVersionId == command.WorkflowVersionId && l.Key == command.Key, cancellationToken);

        if (keyExists)
            throw new InvalidOperationException($"Label with key '{command.Key}' already exists in this workflow version");

        var label = new WorkflowLabelDefinitionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = command.WorkflowVersionId,
            Key = command.Key,
            Type = command.Type,
            Description = command.Description
        };

        _dbContext.Labels.Add(label);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return label.Id;
    }
}


