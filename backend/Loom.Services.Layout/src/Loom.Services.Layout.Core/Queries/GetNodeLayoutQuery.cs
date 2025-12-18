namespace Loom.Services.Layout.Core.Queries;

public record GetNodeLayoutQuery(
    Guid TenantId,
    Guid WorkflowVersionId,
    string NodeKey
);

