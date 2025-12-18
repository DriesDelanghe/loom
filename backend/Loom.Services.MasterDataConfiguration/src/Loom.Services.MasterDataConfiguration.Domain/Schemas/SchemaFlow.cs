namespace Loom.Services.MasterDataConfiguration.Domain.Schemas;

public class SchemaFlow
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SourceSchemaId { get; set; }
    public Guid TargetSchemaId { get; set; }
    public FlowType FlowType { get; set; }
}

