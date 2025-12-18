using Microsoft.EntityFrameworkCore;
using Loom.Services.Layout.Core.Queries;
using Loom.Services.Layout.Domain.Persistence;

namespace Loom.Services.Layout.Core.Queries.Handlers;

public class GetLayoutForWorkflowVersionQueryHandler : IQueryHandler<GetLayoutForWorkflowVersionQuery, WorkflowVersionLayoutDto>
{
    private readonly LayoutDbContext _dbContext;

    public GetLayoutForWorkflowVersionQueryHandler(LayoutDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkflowVersionLayoutDto> HandleAsync(GetLayoutForWorkflowVersionQuery query, CancellationToken cancellationToken = default)
    {
        var layouts = await _dbContext.WorkflowNodeLayouts
            .Where(l => l.TenantId == query.TenantId &&
                       l.WorkflowVersionId == query.WorkflowVersionId)
            .Select(l => new NodeLayoutDto(
                l.NodeKey,
                l.X,
                l.Y,
                l.Width,
                l.Height
            ))
            .ToListAsync(cancellationToken);

        return new WorkflowVersionLayoutDto(layouts);
    }
}

