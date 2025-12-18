using Loom.Services.Configuration.Contracts.Dtos;
using Loom.Services.Configuration.Contracts.Dtos.Commands;
using Loom.Services.Configuration.Contracts.Dtos.Queries;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Core.Queries.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.Configuration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TriggersController : ControllerBase
{
    private readonly CreateTriggerCommandHandler _createTriggerHandler;
    private readonly UpdateTriggerConfigCommandHandler _updateTriggerHandler;
    private readonly DeleteTriggerCommandHandler _deleteTriggerHandler;
    private readonly BindTriggerToWorkflowVersionCommandHandler _bindHandler;
    private readonly UnbindTriggerFromWorkflowVersionCommandHandler _unbindHandler;
    private readonly BindTriggerToNodeCommandHandler _bindToNodeHandler;
    private readonly UnbindTriggerFromNodeCommandHandler _unbindFromNodeHandler;
    private readonly GetWorkflowVersionsForTriggerQueryHandler _getVersionsForTriggerHandler;

    public TriggersController(
        CreateTriggerCommandHandler createTriggerHandler,
        UpdateTriggerConfigCommandHandler updateTriggerHandler,
        DeleteTriggerCommandHandler deleteTriggerHandler,
        BindTriggerToWorkflowVersionCommandHandler bindHandler,
        UnbindTriggerFromWorkflowVersionCommandHandler unbindHandler,
        BindTriggerToNodeCommandHandler bindToNodeHandler,
        UnbindTriggerFromNodeCommandHandler unbindFromNodeHandler,
        GetWorkflowVersionsForTriggerQueryHandler getVersionsForTriggerHandler)
    {
        _createTriggerHandler = createTriggerHandler;
        _updateTriggerHandler = updateTriggerHandler;
        _deleteTriggerHandler = deleteTriggerHandler;
        _bindHandler = bindHandler;
        _unbindHandler = unbindHandler;
        _bindToNodeHandler = bindToNodeHandler;
        _unbindFromNodeHandler = unbindFromNodeHandler;
        _getVersionsForTriggerHandler = getVersionsForTriggerHandler;
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> CreateTrigger(
        [FromBody] CreateTriggerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTriggerCommand(
            request.TenantId,
            request.Type,
            request.Config
        );

        var id = await _createTriggerHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpPut("{triggerId}/config")]
    public async Task<ActionResult<SuccessResponse>> UpdateTriggerConfig(
        Guid triggerId,
        [FromBody] UpdateTriggerConfigRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTriggerConfigCommand(triggerId, request.Config);
        var success = await _updateTriggerHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpDelete("{triggerId}")]
    public async Task<ActionResult<SuccessResponse>> DeleteTrigger(
        Guid triggerId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTriggerCommand(triggerId);
        var success = await _deleteTriggerHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpPost("bind")]
    public async Task<ActionResult<IdResponse>> BindTrigger(
        [FromBody] BindTriggerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BindTriggerToWorkflowVersionCommand(
            request.TriggerId,
            request.WorkflowVersionId,
            request.Priority,
            request.Enabled
        );

        var id = await _bindHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpDelete("bindings/{bindingId}")]
    public async Task<ActionResult<SuccessResponse>> UnbindTrigger(
        Guid bindingId,
        CancellationToken cancellationToken)
    {
        var command = new UnbindTriggerFromWorkflowVersionCommand(bindingId);
        var success = await _unbindHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpPost("bindings/nodes")]
    public async Task<ActionResult<IdResponse>> BindTriggerToNode(
        [FromBody] BindTriggerToNodeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BindTriggerToNodeCommand(
            request.TriggerBindingId,
            request.EntryNodeId,
            request.Order
        );

        var id = await _bindToNodeHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpDelete("bindings/nodes/{nodeBindingId}")]
    public async Task<ActionResult<SuccessResponse>> UnbindTriggerFromNode(
        Guid nodeBindingId,
        CancellationToken cancellationToken)
    {
        var command = new UnbindTriggerFromNodeCommand(nodeBindingId);
        var success = await _unbindFromNodeHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpGet("{triggerId}/workflow-versions")]
    public async Task<ActionResult<List<WorkflowVersionForTriggerResponse>>> GetWorkflowVersionsForTrigger(
        Guid triggerId,
        CancellationToken cancellationToken)
    {
        var query = new GetWorkflowVersionsForTriggerQuery(triggerId);
        var result = await _getVersionsForTriggerHandler.HandleAsync(query, cancellationToken);

        var response = result.Select(v => new WorkflowVersionForTriggerResponse(
            v.WorkflowVersionId,
            v.TenantId,
            v.Priority
        )).ToList();

        return Ok(response);
    }
}

