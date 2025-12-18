namespace Loom.Services.Layout.Core.Commands;

public record UpsertNodeLayoutCommand(
    Guid TenantId,
    Guid WorkflowVersionId,
    string NodeKey,
    decimal X,
    decimal Y,
    decimal? Width,
    decimal? Height
);

