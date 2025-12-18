using Loom.Services.MasterDataConfiguration.Domain.Schemas;

namespace Loom.Services.MasterDataConfiguration.Domain.Persistence;

public class SchemaFlowEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SourceSchemaId { get; set; }
    public Guid TargetSchemaId { get; set; }
    public FlowType FlowType { get; set; }

    public SchemaFlow ToDomain()
    {
        return new SchemaFlow
        {
            Id = Id,
            TenantId = TenantId,
            SourceSchemaId = SourceSchemaId,
            TargetSchemaId = TargetSchemaId,
            FlowType = FlowType
        };
    }

    public static SchemaFlowEntity FromDomain(SchemaFlow flow)
    {
        return new SchemaFlowEntity
        {
            Id = flow.Id,
            TenantId = flow.TenantId,
            SourceSchemaId = flow.SourceSchemaId,
            TargetSchemaId = flow.TargetSchemaId,
            FlowType = flow.FlowType
        };
    }
}
