using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Domain.Persistence;

namespace Loom.Services.Configuration.Core.Queries.Handlers;

public class GetWorkflowVersionsQueryHandler : IQueryHandler<GetWorkflowVersionsQuery, List<WorkflowVersionDto>>
{
    private readonly ConfigurationDbContext _dbContext;

    public GetWorkflowVersionsQueryHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WorkflowVersionDto>> HandleAsync(GetWorkflowVersionsQuery query, CancellationToken cancellationToken = default)
    {
        var versions = await _dbContext.WorkflowVersions
            .Where(v => v.DefinitionId == query.WorkflowDefinitionId)
            .OrderByDescending(v => v.Version)
            .Select(v => new WorkflowVersionDto(
                v.Id,
                v.Version,
                v.Status.ToString(),
                v.CreatedAt,
                v.PublishedAt
            ))
            .ToListAsync(cancellationToken);

        return versions;
    }
}


