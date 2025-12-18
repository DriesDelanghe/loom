using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Commands.Handlers;

public class CreateWorkflowDefinitionCommandHandler : ICommandHandler<CreateWorkflowDefinitionCommand, Guid>
{
    private readonly ConfigurationDbContext _dbContext;

    public CreateWorkflowDefinitionCommandHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(CreateWorkflowDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        var definition = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = command.TenantId,
            Name = command.Name,
            Description = command.Description,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var initialVersion = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Version = 1,
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.WorkflowDefinitions.Add(definition);
        _dbContext.WorkflowVersions.Add(initialVersion);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return definition.Id;
    }
}


