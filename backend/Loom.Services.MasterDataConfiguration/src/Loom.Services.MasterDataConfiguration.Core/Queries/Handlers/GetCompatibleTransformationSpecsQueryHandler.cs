using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class GetCompatibleTransformationSpecsQueryHandler : IQueryHandler<GetCompatibleTransformationSpecsQuery, IReadOnlyList<CompatibleTransformationSpecSummary>>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public GetCompatibleTransformationSpecsQueryHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CompatibleTransformationSpecSummary>> HandleAsync(GetCompatibleTransformationSpecsQuery query, CancellationToken cancellationToken = default)
    {
        var specsQuery = _dbContext.TransformationSpecs
            .Where(s => s.SourceSchemaId == query.SourceSchemaId && s.TargetSchemaId == query.TargetSchemaId);

        if (query.Status.HasValue)
        {
            specsQuery = specsQuery.Where(s => s.Status == query.Status.Value);
        }

        var specs = await specsQuery
            .OrderByDescending(s => s.PublishedAt ?? s.CreatedAt)
            .ToListAsync(cancellationToken);

        // Get schema details for all specs
        var sourceSchemaIds = specs.Select(s => s.SourceSchemaId).Distinct().ToList();
        var targetSchemaIds = specs.Select(s => s.TargetSchemaId).Distinct().ToList();
        var allSchemaIds = sourceSchemaIds.Union(targetSchemaIds).ToList();

        var schemas = await _dbContext.DataSchemas
            .Where(s => allSchemaIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var results = specs.Select(s => new CompatibleTransformationSpecSummary
        {
            Id = s.Id,
            SourceSchemaId = s.SourceSchemaId,
            SourceSchemaKey = schemas.ContainsKey(s.SourceSchemaId) ? schemas[s.SourceSchemaId].Key : "Unknown",
            SourceSchemaVersion = schemas.ContainsKey(s.SourceSchemaId) ? schemas[s.SourceSchemaId].Version : 0,
            TargetSchemaId = s.TargetSchemaId,
            TargetSchemaKey = schemas.ContainsKey(s.TargetSchemaId) ? schemas[s.TargetSchemaId].Key : "Unknown",
            TargetSchemaVersion = schemas.ContainsKey(s.TargetSchemaId) ? schemas[s.TargetSchemaId].Version : 0,
            Mode = s.Mode,
            Cardinality = s.Cardinality,
            Version = s.Version,
            Status = s.Status,
            Description = s.Description,
            PublishedAt = s.PublishedAt
        }).ToList();

        return results;
    }
}

public class CompatibleTransformationSpecSummary
{
    public Guid Id { get; set; }
    public Guid SourceSchemaId { get; set; }
    public string SourceSchemaKey { get; set; } = default!;
    public int SourceSchemaVersion { get; set; }
    public Guid TargetSchemaId { get; set; }
    public string TargetSchemaKey { get; set; } = default!;
    public int TargetSchemaVersion { get; set; }
    public Domain.Transformation.TransformationMode Mode { get; set; }
    public Domain.Transformation.Cardinality Cardinality { get; set; }
    public int Version { get; set; }
    public SchemaStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime? PublishedAt { get; set; }
}

