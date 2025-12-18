using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Domain.Persistence;

namespace Loom.Services.Configuration.Core;

public class ConfigurationDbContext : DbContext
{
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkflowDefinitionEntity> WorkflowDefinitions { get; set; } = null!;
    public DbSet<WorkflowVersionEntity> WorkflowVersions { get; set; } = null!;
    public DbSet<NodeEntity> Nodes { get; set; } = null!;
    public DbSet<ConnectionEntity> Connections { get; set; } = null!;
    public DbSet<WorkflowVariableEntity> Variables { get; set; } = null!;
    public DbSet<WorkflowLabelDefinitionEntity> Labels { get; set; } = null!;
    public DbSet<WorkflowSettingsEntity> Settings { get; set; } = null!;
    public DbSet<TriggerEntity> Triggers { get; set; } = null!;
    public DbSet<TriggerBindingEntity> TriggerBindings { get; set; } = null!;
    public DbSet<TriggerNodeBindingEntity> TriggerNodeBindings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowDefinitionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Name });
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<WorkflowVersionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DefinitionId, e.Version }).IsUnique();
            entity.HasOne(e => e.Definition)
                .WithMany(d => d.Versions)
                .HasForeignKey(e => e.DefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NodeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkflowVersionId, e.Key }).IsUnique();
            entity.HasOne(e => e.WorkflowVersion)
                .WithMany(v => v.Nodes)
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Key).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<ConnectionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkflowVersionId, e.FromNodeId, e.ToNodeId, e.Outcome });
            entity.HasOne(e => e.WorkflowVersion)
                .WithMany(v => v.Connections)
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Outcome).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<WorkflowVariableEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkflowVersionId, e.Key }).IsUnique();
            entity.HasOne(e => e.WorkflowVersion)
                .WithMany(v => v.Variables)
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Key).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<WorkflowLabelDefinitionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkflowVersionId, e.Key }).IsUnique();
            entity.HasOne(e => e.WorkflowVersion)
                .WithMany(v => v.Labels)
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Key).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<WorkflowSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WorkflowVersionId).IsUnique();
            entity.HasOne(e => e.WorkflowVersion)
                .WithOne(v => v.Settings)
                .HasForeignKey<WorkflowSettingsEntity>(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TriggerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
        });

        modelBuilder.Entity<TriggerBindingEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TriggerId, e.WorkflowVersionId });
            entity.HasOne(e => e.Trigger)
                .WithMany(t => t.Bindings)
                .HasForeignKey(e => e.TriggerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.WorkflowVersion)
                .WithMany(v => v.TriggerBindings)
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TriggerNodeBindingEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TriggerBindingId, e.EntryNodeId }).IsUnique();
            entity.HasOne(e => e.TriggerBinding)
                .WithMany(tb => tb.NodeBindings)
                .HasForeignKey(e => e.TriggerBindingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.EntryNode)
                .WithMany()
                .HasForeignKey(e => e.EntryNodeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}


