using Microsoft.EntityFrameworkCore;
using Loom.Services.Layout.Core.Commands;
using Loom.Services.Layout.Core;

namespace Loom.Services.Layout.Core.Commands.Handlers;

public class DeleteNodeLayoutCommandHandler : ICommandHandler<DeleteNodeLayoutCommand, bool>
{
    private readonly LayoutDbContext _dbContext;

    public DeleteNodeLayoutCommandHandler(LayoutDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(DeleteNodeLayoutCommand command, CancellationToken cancellationToken = default)
    {
        var layout = await _dbContext.WorkflowNodeLayouts
            .FirstOrDefaultAsync(
                l => l.TenantId == command.TenantId &&
                     l.WorkflowVersionId == command.WorkflowVersionId &&
                     l.NodeKey == command.NodeKey,
                cancellationToken);

        if (layout != null)
        {
            _dbContext.WorkflowNodeLayouts.Remove(layout);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}

