using Loom.Services.Configuration.Contracts.Dtos;
using Loom.Services.Configuration.Contracts.Dtos.Commands;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.Configuration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NodesController : ControllerBase
{
    private readonly AddNodeCommandHandler _addNodeHandler;
    private readonly UpdateNodeConfigCommandHandler _updateNodeHandler;
    private readonly UpdateNodeMetadataCommandHandler _updateMetadataHandler;
    private readonly RemoveNodeCommandHandler _removeNodeHandler;

    public NodesController(
        AddNodeCommandHandler addNodeHandler,
        UpdateNodeConfigCommandHandler updateNodeHandler,
        UpdateNodeMetadataCommandHandler updateMetadataHandler,
        RemoveNodeCommandHandler removeNodeHandler)
    {
        _addNodeHandler = addNodeHandler;
        _updateNodeHandler = updateNodeHandler;
        _updateMetadataHandler = updateMetadataHandler;
        _removeNodeHandler = removeNodeHandler;
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> AddNode(
        [FromBody] AddNodeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddNodeCommand(
            request.WorkflowVersionId,
            request.Key,
            request.Name,
            request.Type,
            request.Config
        );

        var id = await _addNodeHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpPut("{nodeId}/config")]
    public async Task<ActionResult<SuccessResponse>> UpdateNodeConfig(
        Guid nodeId,
        [FromBody] UpdateNodeConfigRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateNodeConfigCommand(nodeId, request.Config);
        var success = await _updateNodeHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpPut("{nodeId}")]
    public async Task<ActionResult<SuccessResponse>> UpdateNodeMetadata(
        Guid nodeId,
        [FromBody] UpdateNodeMetadataRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateNodeMetadataCommand(nodeId, request.Name, request.Type);
        var success = await _updateMetadataHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpDelete("{nodeId}")]
    public async Task<ActionResult<SuccessResponse>> RemoveNode(
        Guid nodeId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveNodeCommand(nodeId);
        var success = await _removeNodeHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }
}

