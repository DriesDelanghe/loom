using Microsoft.EntityFrameworkCore;
using Loom.Services.Layout.Core.Commands;
using Loom.Services.Layout.Domain.Persistence;

namespace Loom.Services.Layout.Core.Commands.Handlers;

public class UpsertNodeLayoutsBatchCommandHandler : ICommandHandler<UpsertNodeLayoutsBatchCommand, bool>
{
    private readonly LayoutDbContext _dbContext;

    public UpsertNodeLayoutsBatchCommandHandler(LayoutDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UpsertNodeLayoutsBatchCommand command, CancellationToken cancellationToken = default)
    {
        var existingLayouts = await _dbContext.WorkflowNodeLayouts
            .Where(l => l.TenantId == command.TenantId &&
                       l.WorkflowVersionId == command.WorkflowVersionId)
            .ToListAsync(cancellationToken);

        var existingByKey = existingLayouts.ToDictionary(l => l.NodeKey);

        foreach (var layout in command.Layouts)
        {
            if (existingByKey.TryGetValue(layout.NodeKey, out var existing))
            {
                existing.X = layout.X;
                existing.Y = layout.Y;
                existing.Width = layout.Width;
                existing.Height = layout.Height;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _dbContext.WorkflowNodeLayouts.Add(new WorkflowNodeLayoutEntity
                {
                    TenantId = command.TenantId,
                    WorkflowVersionId = command.WorkflowVersionId,
                    NodeKey = layout.NodeKey,
                    X = layout.X,
                    Y = layout.Y,
                    Width = layout.Width,
                    Height = layout.Height,
                    UpdatedAt = DateTime.UtcNow
                });
            }
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

