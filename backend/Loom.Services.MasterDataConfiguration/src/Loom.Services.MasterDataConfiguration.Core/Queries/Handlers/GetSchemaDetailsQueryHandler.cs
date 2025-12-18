using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;

public class GetSchemaDetailsQueryHandler : IQueryHandler<GetSchemaDetailsQuery, DataSchemaDetails?>
{
    private readonly MasterDataConfigurationDbContext _dbContext;

    public GetSchemaDetailsQueryHandler(MasterDataConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataSchemaDetails?> HandleAsync(GetSchemaDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var schema = await _dbContext.DataSchemas
            .Include(s => s.Fields)
            .Include(s => s.Tags)
            .Include(s => s.KeyDefinitions)
                .ThenInclude(k => k.KeyFields)
            .FirstOrDefaultAsync(s => s.Id == query.DataSchemaId, cancellationToken);

        if (schema == null)
            return null;

        return new DataSchemaDetails
        {
            Id = schema.Id,
            TenantId = schema.TenantId,
            DataModelId = schema.DataModelId,
            Role = schema.Role,
            Key = schema.Key,
            Version = schema.Version,
            Status = schema.Status,
            Description = schema.Description,
            CreatedAt = schema.CreatedAt,
            PublishedAt = schema.PublishedAt,
            Fields = schema.Fields.Select(f => new FieldDefinitionSummary
            {
                Id = f.Id,
                Path = f.Path,
                FieldType = f.FieldType,
                ScalarType = f.ScalarType,
                ElementSchemaId = f.ElementSchemaId,
                Required = f.Required,
                Description = f.Description
            }).ToList(),
            Tags = schema.Tags.Select(t => t.Tag).ToList(),
            KeyDefinitions = schema.KeyDefinitions.Select(k => new KeyDefinitionSummary
            {
                Id = k.Id,
                Name = k.Name,
                IsPrimary = k.IsPrimary,
                KeyFields = k.KeyFields.OrderBy(f => f.Order).Select(f => new KeyFieldSummary
                {
                    Id = f.Id,
                    FieldPath = f.FieldPath,
                    Order = f.Order,
                    Normalization = f.Normalization
                }).ToList()
            }).ToList()
        };
    }
}

public class DataSchemaDetails
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? DataModelId { get; set; }
    public Domain.Schemas.SchemaRole Role { get; set; }
    public string Key { get; set; } = default!;
    public int Version { get; set; }
    public Domain.Schemas.SchemaStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public IReadOnlyList<FieldDefinitionSummary> Fields { get; set; } = Array.Empty<FieldDefinitionSummary>();
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
    public IReadOnlyList<KeyDefinitionSummary> KeyDefinitions { get; set; } = Array.Empty<KeyDefinitionSummary>();
}

public class FieldDefinitionSummary
{
    public Guid Id { get; set; }
    public string Path { get; set; } = default!;
    public Domain.Schemas.FieldType FieldType { get; set; }
    public Domain.Schemas.ScalarType? ScalarType { get; set; }
    public Guid? ElementSchemaId { get; set; }
    public bool Required { get; set; }
    public string? Description { get; set; }
}

public class KeyDefinitionSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsPrimary { get; set; }
    public IReadOnlyList<KeyFieldSummary> KeyFields { get; set; } = Array.Empty<KeyFieldSummary>();
}

public class KeyFieldSummary
{
    public Guid Id { get; set; }
    public string FieldPath { get; set; } = default!;
    public int Order { get; set; }
    public string? Normalization { get; set; }
}

