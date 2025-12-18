using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class GetTransformationSpecBySourceSchemaIdQueryHandler : IQueryHandler<GetTransformationSpecBySourceSchemaIdQuery, TransformationSpecDetails?>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public GetTransformationSpecBySourceSchemaIdQueryHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransformationSpecDetails?> HandleAsync(GetTransformationSpecBySourceSchemaIdQuery query, CancellationToken cancellationToken = default)
    {
        var spec = await _dbContext.TransformationSpecs
            .Include(s => s.SimpleRules)
            .Include(s => s.GraphNodes)
            .Include(s => s.GraphEdges)
            .Include(s => s.OutputBindings)
            .Include(s => s.References)
            .Where(s => s.SourceSchemaId == query.SourceSchemaId && s.Mode == Domain.Transformation.TransformationMode.Simple)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);

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
            SimpleRules = spec.SimpleRules.OrderBy(r => r.Order).Select(r => new SimpleTransformRuleSummary
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

