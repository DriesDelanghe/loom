using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Queries.Handlers;

public class GetWorkflowVersionsForTriggerQueryHandler : IQueryHandler<GetWorkflowVersionsForTriggerQuery, List<WorkflowVersionForTriggerDto>>
{
    private readonly ConfigurationDbContext _dbContext;

    public GetWorkflowVersionsForTriggerQueryHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WorkflowVersionForTriggerDto>> HandleAsync(GetWorkflowVersionsForTriggerQuery query, CancellationToken cancellationToken = default)
    {
        var bindings = await _dbContext.TriggerBindings
            .Include(b => b.WorkflowVersion)
            .Include(b => b.Trigger)
            .Where(b => b.TriggerId == query.TriggerId && 
                       b.Enabled && 
                       b.WorkflowVersion.Status == WorkflowStatus.Published)
            .OrderBy(b => b.Priority ?? int.MaxValue)
            .Select(b => new WorkflowVersionForTriggerDto(
                b.WorkflowVersionId,
                b.Trigger.TenantId,
                b.Priority
            ))
            .ToListAsync(cancellationToken);

        return bindings;
    }
}


