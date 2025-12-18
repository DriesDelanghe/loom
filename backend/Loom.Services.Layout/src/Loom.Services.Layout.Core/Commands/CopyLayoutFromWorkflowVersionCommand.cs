namespace Loom.Services.Layout.Core.Commands;

public record CopyLayoutFromWorkflowVersionCommand(
    Guid TenantId,
    Guid SourceWorkflowVersionId,
    Guid TargetWorkflowVersionId
);

