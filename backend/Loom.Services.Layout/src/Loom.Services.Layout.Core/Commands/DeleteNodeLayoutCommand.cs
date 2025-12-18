namespace Loom.Services.Layout.Core.Commands;

public record DeleteNodeLayoutCommand(
    Guid TenantId,
    Guid WorkflowVersionId,
    string NodeKey
);

