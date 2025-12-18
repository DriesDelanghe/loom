using Microsoft.EntityFrameworkCore;
using Loom.Services.Layout.Core.Queries;
using Loom.Services.Layout.Domain.Persistence;

namespace Loom.Services.Layout.Core.Queries.Handlers;

public class GetNodeLayoutQueryHandler : IQueryHandler<GetNodeLayoutQuery, NodeLayoutDto?>
{
    private readonly LayoutDbContext _dbContext;

    public GetNodeLayoutQueryHandler(LayoutDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NodeLayoutDto?> HandleAsync(GetNodeLayoutQuery query, CancellationToken cancellationToken = default)
    {
        var layout = await _dbContext.WorkflowNodeLayouts
            .FirstOrDefaultAsync(
                l => l.TenantId == query.TenantId &&
                     l.WorkflowVersionId == query.WorkflowVersionId &&
                     l.NodeKey == query.NodeKey,
                cancellationToken);

        if (layout == null)
            return null;

        return new NodeLayoutDto(
            layout.NodeKey,
            layout.X,
            layout.Y,
            layout.Width,
            layout.Height
        );
    }
}

