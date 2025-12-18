using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class UpdateWorkflowVariableCommandHandler : ICommandHandler<UpdateWorkflowVariableCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public UpdateWorkflowVariableCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UpdateWorkflowVariableCommand command, CancellationToken cancellationToken = default)
    {
        var variable = await _dbContext.Variables
            .Include(v => v.WorkflowVersion)
            .FirstOrDefaultAsync(v => v.Id == command.VariableId, cancellationToken);

        if (variable == null)
            throw new InvalidOperationException($"Variable {command.VariableId} not found");

        if (variable.WorkflowVersion.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Variables can only be updated in draft versions");

        if (command.Type.HasValue)
            variable.Type = command.Type.Value;

        if (command.InitialValue != null)
            variable.InitialValueJson = command.InitialValue.RootElement.GetRawText();

        if (command.Description != null)
            variable.Description = command.Description;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


