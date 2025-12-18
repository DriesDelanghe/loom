using Microsoft.EntityFrameworkCore;
using Loom.Services.Layout.Domain.Persistence;

namespace Loom.Services.Layout.Core;

public class LayoutDbContext : DbContext
{
    public LayoutDbContext(DbContextOptions<LayoutDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkflowVersionLayoutEntity> WorkflowVersionLayouts { get; set; } = null!;
    public DbSet<WorkflowNodeLayoutEntity> WorkflowNodeLayouts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowVersionLayoutEntity>(entity =>
        {
            entity.HasKey(e => new { e.TenantId, e.WorkflowVersionId });
            entity.HasIndex(e => e.WorkflowVersionId);
        });

        modelBuilder.Entity<WorkflowNodeLayoutEntity>(entity =>
        {
            entity.HasKey(e => new { e.TenantId, e.WorkflowVersionId, e.NodeKey });
            entity.HasIndex(e => new { e.WorkflowVersionId, e.NodeKey });
            entity.Property(e => e.NodeKey).HasMaxLength(200).IsRequired();
        });
    }
}

