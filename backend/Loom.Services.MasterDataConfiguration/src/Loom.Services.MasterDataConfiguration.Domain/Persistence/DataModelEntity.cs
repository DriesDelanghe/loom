using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class DataModelEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public DataModel ToDomain()
    {
        return new DataModel
        {
            Id = Id,
            TenantId = TenantId,
            Key = Key,
            Name = Name,
            Description = Description,
            CreatedAt = CreatedAt
        };
    }

    public static DataModelEntity FromDomain(DataModel model)
    {
        return new DataModelEntity
        {
            Id = model.Id,
            TenantId = model.TenantId,
            Key = model.Key,
            Name = model.Name,
            Description = model.Description,
            CreatedAt = model.CreatedAt
        };
    }
}
