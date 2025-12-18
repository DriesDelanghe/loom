using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core;

public class MasterDataConfigurationDbContext : DbContext
{
    public MasterDataConfigurationDbContext(DbContextOptions<MasterDataConfigurationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DataModelEntity> DataModels { get; set; } = null!;
    public DbSet<DataSchemaEntity> DataSchemas { get; set; } = null!;
    public DbSet<FieldDefinitionEntity> FieldDefinitions { get; set; } = null!;
    public DbSet<SchemaFlowEntity> SchemaFlows { get; set; } = null!;
    public DbSet<SchemaTagEntity> SchemaTags { get; set; } = null!;
    public DbSet<KeyDefinitionEntity> KeyDefinitions { get; set; } = null!;
    public DbSet<KeyFieldEntity> KeyFields { get; set; } = null!;
    public DbSet<ValidationSpecEntity> ValidationSpecs { get; set; } = null!;
    public DbSet<ValidationRuleEntity> ValidationRules { get; set; } = null!;
    public DbSet<ValidationReferenceEntity> ValidationReferences { get; set; } = null!;
    public DbSet<TransformationSpecEntity> TransformationSpecs { get; set; } = null!;
    public DbSet<SimpleTransformRuleEntity> SimpleTransformRules { get; set; } = null!;
    public DbSet<TransformGraphNodeEntity> TransformGraphNodes { get; set; } = null!;
    public DbSet<TransformGraphEdgeEntity> TransformGraphEdges { get; set; } = null!;
    public DbSet<TransformOutputBindingEntity> TransformOutputBindings { get; set; } = null!;
    public DbSet<TransformReferenceEntity> TransformReferences { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataModelEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Key }).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<DataSchemaEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Key, e.Role }).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<FieldDefinitionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DataSchemaId, e.Path }).IsUnique();
            entity.Property(e => e.Path).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasOne(e => e.DataSchema)
                .WithMany(s => s.Fields)
                .HasForeignKey(e => e.DataSchemaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SchemaFlowEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SourceSchemaId, e.TargetSchemaId, e.FlowType }).IsUnique();
        });

        modelBuilder.Entity<SchemaTagEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DataSchemaId, e.Tag }).IsUnique();
            entity.Property(e => e.Tag).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.DataSchema)
                .WithMany(s => s.Tags)
                .HasForeignKey(e => e.DataSchemaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<KeyDefinitionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DataSchemaId, e.Name }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasOne(e => e.DataSchema)
                .WithMany(s => s.KeyDefinitions)
                .HasForeignKey(e => e.DataSchemaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<KeyFieldEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.KeyDefinitionId, e.Order }).IsUnique();
            entity.Property(e => e.FieldPath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Normalization).HasMaxLength(100);
            entity.HasOne(e => e.KeyDefinition)
                .WithMany(k => k.KeyFields)
                .HasForeignKey(e => e.KeyDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ValidationSpecEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DataSchemaId, e.Version }).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<ValidationRuleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Parameters).HasColumnType("jsonb").IsRequired();
            entity.HasOne(e => e.ValidationSpec)
                .WithMany(s => s.Rules)
                .HasForeignKey(e => e.ValidationSpecId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ValidationReferenceEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ParentValidationSpecId, e.FieldPath, e.ChildValidationSpecId }).IsUnique();
            entity.Property(e => e.FieldPath).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.ParentValidationSpec)
                .WithMany(s => s.References)
                .HasForeignKey(e => e.ParentValidationSpecId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransformationSpecEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SourceSchemaId, e.TargetSchemaId, e.Version }).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<SimpleTransformRuleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourcePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.TargetPath).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.TransformationSpec)
                .WithMany(s => s.SimpleRules)
                .HasForeignKey(e => e.TransformationSpecId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransformGraphNodeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TransformationSpecId, e.Key }).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(200).IsRequired();
            entity.Property(e => e.OutputType).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.Config).HasColumnType("jsonb").IsRequired();
            entity.HasOne(e => e.TransformationSpec)
                .WithMany(s => s.GraphNodes)
                .HasForeignKey(e => e.TransformationSpecId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransformGraphEdgeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InputName).HasMaxLength(200).IsRequired();
            entity.HasOne(e => e.TransformationSpec)
                .WithMany(s => s.GraphEdges)
                .HasForeignKey(e => e.TransformationSpecId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransformOutputBindingEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TransformationSpecId, e.TargetPath }).IsUnique();
            entity.Property(e => e.TargetPath).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.TransformationSpec)
                .WithMany(s => s.OutputBindings)
                .HasForeignKey(e => e.TransformationSpecId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TransformReferenceEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ParentTransformationSpecId, e.SourceFieldPath, e.TargetFieldPath }).IsUnique();
            entity.Property(e => e.SourceFieldPath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.TargetFieldPath).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.ParentTransformationSpec)
                .WithMany(s => s.References)
                .HasForeignKey(e => e.ParentTransformationSpecId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
