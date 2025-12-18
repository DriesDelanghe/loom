using Microsoft.EntityFrameworkCore;
using Loom.Services.Layout.Core.Commands;
using Loom.Services.Layout.Domain.Persistence;

namespace Loom.Services.Layout.Core.Commands.Handlers;

public class CopyLayoutFromWorkflowVersionCommandHandler : ICommandHandler<CopyLayoutFromWorkflowVersionCommand, bool>
{
    private readonly LayoutDbContext _dbContext;

    public CopyLayoutFromWorkflowVersionCommandHandler(LayoutDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(CopyLayoutFromWorkflowVersionCommand command, CancellationToken cancellationToken = default)
    {
        var sourceLayouts = await _dbContext.WorkflowNodeLayouts
            .Where(l => l.TenantId == command.TenantId &&
                       l.WorkflowVersionId == command.SourceWorkflowVersionId)
            .ToListAsync(cancellationToken);

        var existingTargetLayouts = await _dbContext.WorkflowNodeLayouts
            .Where(l => l.TenantId == command.TenantId &&
                       l.WorkflowVersionId == command.TargetWorkflowVersionId)
            .ToListAsync(cancellationToken);

        var existingByKey = existingTargetLayouts.ToDictionary(l => l.NodeKey);

        foreach (var sourceLayout in sourceLayouts)
        {
            if (existingByKey.TryGetValue(sourceLayout.NodeKey, out var existing))
            {
                existing.X = sourceLayout.X;
                existing.Y = sourceLayout.Y;
                existing.Width = sourceLayout.Width;
                existing.Height = sourceLayout.Height;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _dbContext.WorkflowNodeLayouts.Add(new WorkflowNodeLayoutEntity
                {
                    TenantId = command.TenantId,
                    WorkflowVersionId = command.TargetWorkflowVersionId,
                    NodeKey = sourceLayout.NodeKey,
                    X = sourceLayout.X,
                    Y = sourceLayout.Y,
                    Width = sourceLayout.Width,
                    Height = sourceLayout.Height,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        var versionLayout = await _dbContext.WorkflowVersionLayouts
            .FirstOrDefaultAsync(
                l => l.TenantId == command.TenantId &&
                     l.WorkflowVersionId == command.TargetWorkflowVersionId,
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
                WorkflowVersionId = command.TargetWorkflowVersionId,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

