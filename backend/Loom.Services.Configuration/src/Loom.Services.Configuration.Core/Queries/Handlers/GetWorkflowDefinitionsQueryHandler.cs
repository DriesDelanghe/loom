using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Queries.Handlers;

public class GetWorkflowDefinitionsQueryHandler : IQueryHandler<GetWorkflowDefinitionsQuery, List<WorkflowDefinitionDto>>
{
    private readonly ConfigurationDbContext _dbContext;

    public GetWorkflowDefinitionsQueryHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WorkflowDefinitionDto>> HandleAsync(GetWorkflowDefinitionsQuery query, CancellationToken cancellationToken = default)
    {
        var definitions = await _dbContext.WorkflowDefinitions
            .Where(d => d.TenantId == query.TenantId && !d.IsArchived)
            .Select(d => new
            {
                d.Id,
                d.Name,
                HasPublished = d.Versions.Any(v => v.Status == WorkflowStatus.Published),
                LatestVersion = d.Versions.OrderByDescending(v => v.Version).Select(v => (int?)v.Version).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return definitions.Select(d => new WorkflowDefinitionDto(
            d.Id,
            d.Name,
            d.HasPublished,
            d.LatestVersion
        )).ToList();
    }
}


