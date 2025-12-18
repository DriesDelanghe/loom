using Microsoft.EntityFrameworkCore;
using Loom.Services.Layout.Core.Commands;
using Loom.Services.Layout.Domain.Persistence;

namespace Loom.Services.Layout.Core.Commands.Handlers;

public class UpsertNodeLayoutCommandHandler : ICommandHandler<UpsertNodeLayoutCommand, bool>
{
    private readonly LayoutDbContext _dbContext;

    public UpsertNodeLayoutCommandHandler(LayoutDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UpsertNodeLayoutCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.WorkflowNodeLayouts
            .FirstOrDefaultAsync(
                l => l.TenantId == command.TenantId &&
                     l.WorkflowVersionId == command.WorkflowVersionId &&
                     l.NodeKey == command.NodeKey,
                cancellationToken);

        if (existing != null)
        {
            existing.X = command.X;
            existing.Y = command.Y;
            existing.Width = command.Width;
            existing.Height = command.Height;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _dbContext.WorkflowNodeLayouts.Add(new WorkflowNodeLayoutEntity
            {
                TenantId = command.TenantId,
                WorkflowVersionId = command.WorkflowVersionId,
                NodeKey = command.NodeKey,
                X = command.X,
                Y = command.Y,
                Width = command.Width,
                Height = command.Height,
                UpdatedAt = DateTime.UtcNow
            });
        }

        var versionLayout = await _dbContext.WorkflowVersionLayouts
            .FirstOrDefaultAsync(
                l => l.TenantId == command.TenantId &&
                     l.WorkflowVersionId == command.WorkflowVersionId,
                cancellationToken);

        if (versionLayout != null)
        {
            versionLayout.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _dbContext.WorkflowVersionLayouts.Add(new WorkflowVersionLayoutEntity
            {
                TenantId = command.TenantId,
                WorkflowVersionId = command.WorkflowVersionId,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

