using Loom.Services.Configuration.Contracts.Dtos;
using Loom.Services.Configuration.Contracts.Dtos.Commands;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.Configuration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VariablesController : ControllerBase
{
    private readonly AddWorkflowVariableCommandHandler _addVariableHandler;
    private readonly UpdateWorkflowVariableCommandHandler _updateVariableHandler;
    private readonly RemoveWorkflowVariableCommandHandler _removeVariableHandler;

    public VariablesController(
        AddWorkflowVariableCommandHandler addVariableHandler,
        UpdateWorkflowVariableCommandHandler updateVariableHandler,
        RemoveWorkflowVariableCommandHandler removeVariableHandler)
    {
        _addVariableHandler = addVariableHandler;
        _updateVariableHandler = updateVariableHandler;
        _removeVariableHandler = removeVariableHandler;
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> AddVariable(
        [FromBody] AddWorkflowVariableRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddWorkflowVariableCommand(
            request.WorkflowVersionId,
            request.Key,
            request.Type,
            request.InitialValue,
            request.Description
        );

        var id = await _addVariableHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpPut("{variableId}")]
    public async Task<ActionResult<SuccessResponse>> UpdateVariable(
        Guid variableId,
        [FromBody] UpdateWorkflowVariableRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateWorkflowVariableCommand(
            variableId,
            request.Type,
            request.InitialValue,
            request.Description
        );

        var success = await _updateVariableHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpDelete("{variableId}")]
    public async Task<ActionResult<SuccessResponse>> RemoveVariable(
        Guid variableId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveWorkflowVariableCommand(variableId);
        var success = await _removeVariableHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }
}

