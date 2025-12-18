namespace Loom.Services.Layout.Core.Queries;

public record GetLayoutForWorkflowVersionQuery(
    Guid TenantId,
    Guid WorkflowVersionId
);

public record NodeLayoutDto(
    string NodeKey,
    decimal X,
    decimal Y,
    decimal? Width,
    decimal? Height
);

public record WorkflowVersionLayoutDto(
    List<NodeLayoutDto> Nodes
);

