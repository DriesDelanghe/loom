using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class GetCompiledTransformationSpecQueryHandler : IQueryHandler<GetCompiledTransformationSpecQuery, CompiledTransformationSpec?>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public GetCompiledTransformationSpecQueryHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CompiledTransformationSpec?> HandleAsync(GetCompiledTransformationSpecQuery query, CancellationToken cancellationToken = default)
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

        if (spec.Status != Domain.Schemas.SchemaStatus.Published)
            throw new InvalidOperationException($"Transformation spec must be Published to be compiled. Current status: {spec.Status}");

        return new CompiledTransformationSpec
        {
            Id = spec.Id,
            SourceSchemaId = spec.SourceSchemaId,
            TargetSchemaId = spec.TargetSchemaId,
            Mode = spec.Mode,
            Cardinality = spec.Cardinality,
            SimpleRules = spec.SimpleRules.OrderBy(r => r.Order).Select(r => new CompiledSimpleTransformRule
            {
                SourcePath = r.SourcePath,
                TargetPath = r.TargetPath,
                ConverterId = r.ConverterId,
                Required = r.Required
            }).ToList(),
            GraphNodes = spec.GraphNodes.ToDictionary(n => n.Key, n => new CompiledTransformGraphNode
            {
                NodeType = n.NodeType,
                OutputType = n.OutputType,
                Config = n.Config
            }),
            GraphEdges = spec.GraphEdges.OrderBy(e => e.Order).Select(e => new CompiledTransformGraphEdge
            {
                FromNodeKey = spec.GraphNodes.First(n => n.Id == e.FromNodeId).Key,
                ToNodeKey = spec.GraphNodes.First(n => n.Id == e.ToNodeId).Key,
                InputName = e.InputName
            }).ToList(),
            OutputBindings = spec.OutputBindings.Select(b => new CompiledTransformOutputBinding
            {
                TargetPath = b.TargetPath,
                FromNodeKey = spec.GraphNodes.First(n => n.Id == b.FromNodeId).Key
            }).ToList(),
            References = spec.References.Select(r => new CompiledTransformReference
            {
                SourceFieldPath = r.SourceFieldPath,
                TargetFieldPath = r.TargetFieldPath,
                ChildTransformationSpecId = r.ChildTransformationSpecId,
                ElementScoped = true // TransformReferences always apply per-element for arrays
            }).ToList()
        };
    }
}

public class CompiledTransformationSpec
{
    public Guid Id { get; set; }
    public Guid SourceSchemaId { get; set; }
    public Guid TargetSchemaId { get; set; }
    public Domain.Transformation.TransformationMode Mode { get; set; }
    public Domain.Transformation.Cardinality Cardinality { get; set; }
    public IReadOnlyList<CompiledSimpleTransformRule> SimpleRules { get; set; } = Array.Empty<CompiledSimpleTransformRule>();
    public Dictionary<string, CompiledTransformGraphNode> GraphNodes { get; set; } = new();
    public IReadOnlyList<CompiledTransformGraphEdge> GraphEdges { get; set; } = Array.Empty<CompiledTransformGraphEdge>();
    public IReadOnlyList<CompiledTransformOutputBinding> OutputBindings { get; set; } = Array.Empty<CompiledTransformOutputBinding>();
    public IReadOnlyList<CompiledTransformReference> References { get; set; } = Array.Empty<CompiledTransformReference>();
}

public class CompiledSimpleTransformRule
{
    public string SourcePath { get; set; } = default!;
    public string TargetPath { get; set; } = default!;
    public Guid? ConverterId { get; set; }
    public bool Required { get; set; }
}

public class CompiledTransformGraphNode
{
    public Domain.Transformation.TransformNodeType NodeType { get; set; }
    public string OutputType { get; set; } = default!;
    public string Config { get; set; } = default!;
}

public class CompiledTransformGraphEdge
{
    public string FromNodeKey { get; set; } = default!;
    public string ToNodeKey { get; set; } = default!;
    public string InputName { get; set; } = default!;
}

public class CompiledTransformOutputBinding
{
    public string TargetPath { get; set; } = default!;
    public string FromNodeKey { get; set; } = default!;
}

public class CompiledTransformReference
{
    public string SourceFieldPath { get; set; } = default!;
    public string TargetFieldPath { get; set; } = default!;
    public Guid ChildTransformationSpecId { get; set; }
    /// <summary>
    /// Indicates that this transformation applies per-element for array fields.
    /// For arrays, the child transformation is applied to each element.
    /// </summary>
    public bool ElementScoped { get; set; }
}


