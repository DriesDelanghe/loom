using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class ArchiveWorkflowDefinitionCommandHandler : ICommandHandler<ArchiveWorkflowDefinitionCommand, bool>
{
    private readonly ConfigurationDbContext _dbContext;

    public ArchiveWorkflowDefinitionCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(ArchiveWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var definition = await _dbContext.WorkflowDefinitions
            .FirstOrDefaultAsync(d => d.Id == command.WorkflowDefinitionId, cancellationToken);

        if (definition == null)
            throw new InvalidOperationException($"Workflow definition {command.WorkflowDefinitionId} not found");

        definition.IsArchived = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}


