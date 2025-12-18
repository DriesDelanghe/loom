using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class GetTransformationSpecDetailsQueryHandler : IQueryHandler<GetTransformationSpecDetailsQuery, TransformationSpecDetails?>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public GetTransformationSpecDetailsQueryHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransformationSpecDetails?> HandleAsync(GetTransformationSpecDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.TransformationSpecs
            .Include(s => s.SimpleRules)
            .Include(s => s.GraphNodes)
            .Include(s => s.GraphEdges)
            .Include(s => s.OutputBindings)
            .Include(s => s.References)
            .FirstOrDefaultAsync(s => s.Id == query.TransformationSpecId, cancellationToken);

        if (spec == null)
            return null;

        return new TransformationSpecDetails
        {
            Id = spec.Id,
            TenantId = spec.TenantId,
            SourceSchemaId = spec.SourceSchemaId,
            TargetSchemaId = spec.TargetSchemaId,
            Mode = spec.Mode,
            Cardinality = spec.Cardinality,
            Version = spec.Version,
            Status = spec.Status,
            Description = spec.Description,
            CreatedAt = spec.CreatedAt,
            PublishedAt = spec.PublishedAt,
            SimpleRules = spec.SimpleRules.Select(r => new SimpleTransformRuleSummary
            {
                Id = r.Id,
                SourcePath = r.SourcePath,
                TargetPath = r.TargetPath,
                ConverterId = r.ConverterId,
                Required = r.Required,
                Order = r.Order
            }).ToList(),
            GraphNodes = spec.GraphNodes.Select(n => new TransformGraphNodeSummary
            {
                Id = n.Id,
                Key = n.Key,
                NodeType = n.NodeType,
                OutputType = n.OutputType,
                Config = n.Config
            }).ToList(),
            GraphEdges = spec.GraphEdges.Select(e => new TransformGraphEdgeSummary
            {
                Id = e.Id,
                FromNodeId = e.FromNodeId,
                ToNodeId = e.ToNodeId,
                InputName = e.InputName,
                Order = e.Order
            }).ToList(),
            OutputBindings = spec.OutputBindings.Select(b => new TransformOutputBindingSummary
            {
                Id = b.Id,
                TargetPath = b.TargetPath,
                FromNodeId = b.FromNodeId
            }).ToList(),
            References = spec.References.Select(r => new TransformReferenceSummary
            {
                Id = r.Id,
                SourceFieldPath = r.SourceFieldPath,
                TargetFieldPath = r.TargetFieldPath,
                ChildTransformationSpecId = r.ChildTransformationSpecId
            }).ToList()
        };
    }
}

public class TransformationSpecDetails
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SourceSchemaId { get; set; }
    public Guid TargetSchemaId { get; set; }
    public Domain.Transformation.TransformationMode Mode { get; set; }
    public Domain.Transformation.Cardinality Cardinality { get; set; }
    public int Version { get; set; }
    public Domain.Schemas.SchemaStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public IReadOnlyList<SimpleTransformRuleSummary> SimpleRules { get; set; } = Array.Empty<SimpleTransformRuleSummary>();
    public IReadOnlyList<TransformGraphNodeSummary> GraphNodes { get; set; } = Array.Empty<TransformGraphNodeSummary>();
    public IReadOnlyList<TransformGraphEdgeSummary> GraphEdges { get; set; } = Array.Empty<TransformGraphEdgeSummary>();
    public IReadOnlyList<TransformOutputBindingSummary> OutputBindings { get; set; } = Array.Empty<TransformOutputBindingSummary>();
    public IReadOnlyList<TransformReferenceSummary> References { get; set; } = Array.Empty<TransformReferenceSummary>();
}

public class SimpleTransformRuleSummary
{
    public Guid Id { get; set; }
    public string SourcePath { get; set; } = default!;
    public string TargetPath { get; set; } = default!;
    public Guid? ConverterId { get; set; }
    public bool Required { get; set; }
    public int Order { get; set; }
}

public class TransformGraphNodeSummary
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public Domain.Transformation.TransformNodeType NodeType { get; set; }
    public string OutputType { get; set; } = default!;
    public string Config { get; set; } = default!;
}

public class TransformGraphEdgeSummary
{
    public Guid Id { get; set; }
    public Guid FromNodeId { get; set; }
    public Guid ToNodeId { get; set; }
    public string InputName { get; set; } = default!;
    public int Order { get; set; }
}

public class TransformOutputBindingSummary
{
    public Guid Id { get; set; }
    public string TargetPath { get; set; } = default!;
    public Guid FromNodeId { get; set; }
}

public class TransformReferenceSummary
{
    public Guid Id { get; set; }
    public string SourceFieldPath { get; set; } = default!;
    public string TargetFieldPath { get; set; } = default!;
    public Guid ChildTransformationSpecId { get; set; }
}


