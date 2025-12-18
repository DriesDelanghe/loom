using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;
using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class GetUnpublishedDependenciesQueryHandler : IQueryHandler<GetUnpublishedDependenciesQuery, IReadOnlyList<UnpublishedDependencyDto>>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public GetUnpublishedDependenciesQueryHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UnpublishedDependencyDto>> HandleAsync(GetUnpublishedDependenciesQuery query, CancellationToken cancellationToken = default)
    {
        var schema = await _dbContext.DataSchemas
            .Include(s => s.Fields)
            .FirstOrDefaultAsync(s => s.Id == query.SchemaId, cancellationToken);

        if (schema == null)
            throw new InvalidOperationException($"Schema {query.SchemaId} not found");

        var referencedSchemaIds = schema.Fields
            .Where(f => f.ElementSchemaId.HasValue)
            .Select(f => f.ElementSchemaId!.Value)
            .Distinct()
            .ToList();

        if (referencedSchemaIds.Count == 0)
            return Array.Empty<UnpublishedDependencyDto>();

        var referencedSchemas = await _dbContext.DataSchemas
            .Where(s => referencedSchemaIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var unpublished = referencedSchemas
            .Where(s => s.Status != SchemaStatus.Published)
            .Select(s => new UnpublishedDependencyDto
            {
                SchemaId = s.Id,
                Key = s.Key,
                Version = s.Version,
                Status = s.Status,
                Role = s.Role
            })
            .ToList();

        return unpublished;
    }
}

public class UnpublishedDependencyDto
{
    public Guid SchemaId { get; set; }
    public string Key { get; set; } = default!;
    public int Version { get; set; }
    public SchemaStatus Status { get; set; }
    public SchemaRole Role { get; set; }
}

