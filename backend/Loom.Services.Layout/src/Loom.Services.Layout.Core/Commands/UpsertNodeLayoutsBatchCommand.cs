namespace Loom.Services.Layout.Core.Commands;

public record NodeLayoutData(
    string NodeKey,
    decimal X,
    decimal Y,
    decimal? Width,
    decimal? Height
);

public record UpsertNodeLayoutsBatchCommand(
    Guid TenantId,
    Guid WorkflowVersionId,
    List<NodeLayoutData> Layouts
);

