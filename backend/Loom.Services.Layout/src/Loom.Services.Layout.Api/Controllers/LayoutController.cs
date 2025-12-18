using Loom.Services.Layout.Core.Commands;
using Loom.Services.Layout.Core.Commands.Handlers;
using Loom.Services.Layout.Core.Queries;
using Loom.Services.Layout.Core.Queries.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.Layout.Api.Controllers;

[ApiController]
[Route("api/workflow-versions/{workflowVersionId}/layout")]
public class LayoutController : ControllerBase
{
    private readonly UpsertNodeLayoutCommandHandler _upsertNodeLayoutHandler;
    private readonly UpsertNodeLayoutsBatchCommandHandler _upsertBatchHandler;
    private readonly CopyLayoutFromWorkflowVersionCommandHandler _copyLayoutHandler;
    private readonly DeleteNodeLayoutCommandHandler _deleteNodeLayoutHandler;
    private readonly GetLayoutForWorkflowVersionQueryHandler _getLayoutHandler;
    private readonly GetNodeLayoutQueryHandler _getNodeLayoutHandler;

    private const string TENANT_ID_HEADER = "X-Tenant-Id";
    private static readonly Guid DEFAULT_TENANT_ID = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public LayoutController(
        UpsertNodeLayoutCommandHandler upsertNodeLayoutHandler,
        UpsertNodeLayoutsBatchCommandHandler upsertBatchHandler,
        CopyLayoutFromWorkflowVersionCommandHandler copyLayoutHandler,
        DeleteNodeLayoutCommandHandler deleteNodeLayoutHandler,
        GetLayoutForWorkflowVersionQueryHandler getLayoutHandler,
        GetNodeLayoutQueryHandler getNodeLayoutHandler)
    {
        _upsertNodeLayoutHandler = upsertNodeLayoutHandler;
        _upsertBatchHandler = upsertBatchHandler;
        _copyLayoutHandler = copyLayoutHandler;
        _deleteNodeLayoutHandler = deleteNodeLayoutHandler;
        _getLayoutHandler = getLayoutHandler;
        _getNodeLayoutHandler = getNodeLayoutHandler;
    }

    private Guid GetTenantId()
    {
        if (Request.Headers.TryGetValue(TENANT_ID_HEADER, out var tenantIdHeader) &&
            Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            return tenantId;
        }
        return DEFAULT_TENANT_ID;
    }

    [HttpGet]
    public async Task<ActionResult<WorkflowVersionLayoutResponse>> GetLayout(
        Guid workflowVersionId,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var query = new GetLayoutForWorkflowVersionQuery(tenantId, workflowVersionId);
        var result = await _getLayoutHandler.HandleAsync(query, cancellationToken);

        var response = new WorkflowVersionLayoutResponse(
            result.Nodes.Select(n => new NodeLayoutResponse(
                n.NodeKey,
                n.X,
                n.Y,
                n.Width,
                n.Height
            )).ToList()
        );

        return Ok(response);
    }

    [HttpPut("nodes/{nodeKey}")]
    public async Task<ActionResult<SuccessResponse>> UpsertNodeLayout(
        Guid workflowVersionId,
        string nodeKey,
        [FromBody] UpsertNodeLayoutRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var command = new UpsertNodeLayoutCommand(
            tenantId,
            workflowVersionId,
            nodeKey,
            request.X,
            request.Y,
            request.Width,
            request.Height
        );

        var success = await _upsertNodeLayoutHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpPut("nodes")]
    public async Task<ActionResult<SuccessResponse>> UpsertNodeLayoutsBatch(
        Guid workflowVersionId,
        [FromBody] UpsertNodeLayoutsBatchRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var command = new UpsertNodeLayoutsBatchCommand(
            tenantId,
            workflowVersionId,
            request.Nodes.Select(n => new NodeLayoutData(
                n.NodeKey,
                n.X,
                n.Y,
                n.Width,
                n.Height
            )).ToList()
        );

        var success = await _upsertBatchHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpPost("copy-from/{sourceWorkflowVersionId}")]
    public async Task<ActionResult<SuccessResponse>> CopyLayoutFromWorkflowVersion(
        Guid workflowVersionId,
        Guid sourceWorkflowVersionId,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var command = new CopyLayoutFromWorkflowVersionCommand(
            tenantId,
            sourceWorkflowVersionId,
            workflowVersionId
        );

        var success = await _copyLayoutHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpDelete("nodes/{nodeKey}")]
    public async Task<ActionResult<SuccessResponse>> DeleteNodeLayout(
        Guid workflowVersionId,
        string nodeKey,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var command = new DeleteNodeLayoutCommand(tenantId, workflowVersionId, nodeKey);
        var success = await _deleteNodeLayoutHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }
}

public record SuccessResponse(bool Success);
public record NodeLayoutResponse(string NodeKey, decimal X, decimal Y, decimal? Width, decimal? Height);
public record WorkflowVersionLayoutResponse(List<NodeLayoutResponse> Nodes);
public record UpsertNodeLayoutRequest(decimal X, decimal Y, decimal? Width, decimal? Height);
public record UpsertNodeLayoutsBatchRequest(List<NodeLayoutRequest> Nodes);
public record NodeLayoutRequest(string NodeKey, decimal X, decimal Y, decimal? Width, decimal? Height);

