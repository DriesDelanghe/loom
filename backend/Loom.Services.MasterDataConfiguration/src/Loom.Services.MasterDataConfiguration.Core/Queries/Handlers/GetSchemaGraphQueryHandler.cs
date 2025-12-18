using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class GetSchemaGraphQueryHandler : IQueryHandler<GetSchemaGraphQuery, SchemaGraph?>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public GetSchemaGraphQueryHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SchemaGraph?> HandleAsync(GetSchemaGraphQuery query, CancellationToken cancellationToken = default)
    {
        var schema = await _dbContext.DataSchemas
            .Include(s => s.Fields)
            .FirstOrDefaultAsync(s => s.Id == query.DataSchemaId, cancellationToken);

        if (schema == null)
            return null;

        var referencedSchemaIds = schema.Fields
            .Where(f => f.ElementSchemaId.HasValue)
            .Select(f => f.ElementSchemaId!.Value)
            .Distinct()
            .ToList();

        var referencedSchemas = await _dbContext.DataSchemas
            .Where(s => referencedSchemaIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var nodes = new List<SchemaGraphNode>
        {
            new SchemaGraphNode
            {
                SchemaId = schema.Id,
                Key = schema.Key,
                Version = schema.Version,
                Role = schema.Role,
                Status = schema.Status
            }
        };

        nodes.AddRange(referencedSchemas.Select(s => new SchemaGraphNode
        {
            SchemaId = s.Id,
            Key = s.Key,
            Version = s.Version,
            Role = s.Role,
            Status = s.Status
        }));

        var edges = schema.Fields
            .Where(f => f.ElementSchemaId.HasValue)
            .Select(f => new SchemaGraphEdge
            {
                FromSchemaId = schema.Id,
                ToSchemaId = f.ElementSchemaId!.Value,
                FieldPath = f.Path
            })
            .ToList();

        return new SchemaGraph
        {
            RootSchemaId = schema.Id,
            Nodes = nodes,
            Edges = edges
        };
    }
}

public class SchemaGraph
{
    public Guid RootSchemaId { get; set; }
    public IReadOnlyList<SchemaGraphNode> Nodes { get; set; } = Array.Empty<SchemaGraphNode>();
    public IReadOnlyList<SchemaGraphEdge> Edges { get; set; } = Array.Empty<SchemaGraphEdge>();
}

public class SchemaGraphNode
{
    public Guid SchemaId { get; set; }
    public string Key { get; set; } = default!;
    public int Version { get; set; }
    public Domain.Schemas.SchemaRole Role { get; set; }
    public Domain.Schemas.SchemaStatus Status { get; set; }
}

public class SchemaGraphEdge
{
    public Guid FromSchemaId { get; set; }
    public Guid ToSchemaId { get; set; }
    public string FieldPath { get; set; } = default!;
}


