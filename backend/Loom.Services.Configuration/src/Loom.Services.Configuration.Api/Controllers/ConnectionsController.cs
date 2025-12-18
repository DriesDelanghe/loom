using Loom.Services.Configuration.Contracts.Dtos;
using Loom.Services.Configuration.Contracts.Dtos.Commands;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.Configuration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConnectionsController : ControllerBase
{
    private readonly AddConnectionCommandHandler _addConnectionHandler;
    private readonly RemoveConnectionCommandHandler _removeConnectionHandler;

    public ConnectionsController(
        AddConnectionCommandHandler addConnectionHandler,
        RemoveConnectionCommandHandler removeConnectionHandler)
    {
        _addConnectionHandler = addConnectionHandler;
        _removeConnectionHandler = removeConnectionHandler;
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> AddConnection(
        [FromBody] AddConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddConnectionCommand(
            request.WorkflowVersionId,
            request.FromNodeId,
            request.ToNodeId,
            request.Outcome,
            request.Order
        );

        var id = await _addConnectionHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpDelete("{connectionId:guid}")]
    public async Task<ActionResult<SuccessResponse>> RemoveConnection(
        Guid connectionId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveConnectionCommand(connectionId);
        var success = await _removeConnectionHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }
}

