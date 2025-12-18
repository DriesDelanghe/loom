using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class GetSchemasQueryHandler : IQueryHandler<GetSchemasQuery, IReadOnlyList<DataSchemaSummary>>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public GetSchemasQueryHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DataSchemaSummary>> HandleAsync(GetSchemasQuery query, CancellationToken cancellationToken = default)
    {
        var schemas = _dbContext.DataSchemas
            .Where(s => s.TenantId == query.TenantId);

        if (query.Role.HasValue)
        {
            schemas = schemas.Where(s => s.Role == query.Role.Value);
        }

        if (query.Status.HasValue)
        {
            schemas = schemas.Where(s => s.Status == query.Status.Value);
        }

        var results = await schemas
            .Include(s => s.Tags)
            .OrderBy(s => s.Key)
            .ThenByDescending(s => s.Version)
            .Select(s => new DataSchemaSummary
            {
                Id = s.Id,
                TenantId = s.TenantId,
                DataModelId = s.DataModelId,
                Role = s.Role,
                Key = s.Key,
                Version = s.Version,
                Status = s.Status,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                PublishedAt = s.PublishedAt,
                Tags = s.Tags.Select(t => t.Tag).ToList()
            })
            .ToListAsync(cancellationToken);

        return results;
    }
}

public class DataSchemaSummary
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? DataModelId { get; set; }
    public SchemaRole Role { get; set; }
    public string Key { get; set; } = default!;
    public int Version { get; set; }
    public SchemaStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
}

