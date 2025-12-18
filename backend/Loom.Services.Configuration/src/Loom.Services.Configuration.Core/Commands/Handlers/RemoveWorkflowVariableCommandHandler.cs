using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class RemoveWorkflowVariableCommandHandler : ICommandHandler<RemoveWorkflowVariableCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public RemoveWorkflowVariableCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(RemoveWorkflowVariableCommand command, CancellationToken cancellationToken = default)
    {
        var variable = await _dbContext.Variables
            .Include(v => v.WorkflowVersion)
            .FirstOrDefaultAsync(v => v.Id == command.VariableId, cancellationToken);

        if (variable == null)
            throw new InvalidOperationException($"Variable {command.VariableId} not found");

        if (variable.WorkflowVersion.Status != WorkflowStatus.Draft)
            throw new InvalidOperationException("Variables can only be removed from draft versions");

        _dbContext.Variables.Remove(variable);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


