using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class AddWorkflowVariableCommandHandler : ICommandHandler<AddWorkflowVariableCommand, Guid>
{
    private readonly ConfigurationDbContext _dbContext;

    public AddWorkflowVariableCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(AddWorkflowVariableCommand command, CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.WorkflowVersions
            .FirstOrDefaultAsync(v => v.Id == command.WorkflowVersionId, cancellationToken);

        if (version == null)
            throw new InvalidOperationException($"Workflow version {command.WorkflowVersionId} not found");

        if (version.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Variables can only be added to draft versions");

        var keyExists = await _dbContext.Variables
            .AnyAsync(v => v.WorkflowVersionId == command.WorkflowVersionId && v.Key == command.Key, cancellationToken);

        if (keyExists)
            throw new InvalidOperationException($"Variable with key '{command.Key}' already exists in this workflow version");

        var variable = new WorkflowVariableEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = command.WorkflowVersionId,
            Key = command.Key,
            Type = command.Type,
            InitialValueJson = command.InitialValue?.RootElement.GetRawText(),
            Description = command.Description
        };

        _dbContext.Variables.Add(variable);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return variable.Id;
    }
}


