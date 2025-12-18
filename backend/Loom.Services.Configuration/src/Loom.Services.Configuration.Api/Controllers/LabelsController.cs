using Loom.Services.Configuration.Contracts.Dtos;
using Loom.Services.Configuration.Contracts.Dtos.Commands;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.Configuration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabelsController : ControllerBase
{
    private readonly AddWorkflowLabelDefinitionCommandHandler _addLabelHandler;
    private readonly RemoveWorkflowLabelDefinitionCommandHandler _removeLabelHandler;

    public LabelsController(
        AddWorkflowLabelDefinitionCommandHandler addLabelHandler,
        RemoveWorkflowLabelDefinitionCommandHandler removeLabelHandler)
    {
        _addLabelHandler = addLabelHandler;
        _removeLabelHandler = removeLabelHandler;
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> AddLabel(
        [FromBody] AddWorkflowLabelRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddWorkflowLabelDefinitionCommand(
            request.WorkflowVersionId,
            request.Key,
            request.Type,
            request.Description
        );

        var id = await _addLabelHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpDelete("{labelId}")]
    public async Task<ActionResult<SuccessResponse>> RemoveLabel(
        Guid labelId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveWorkflowLabelDefinitionCommand(labelId);
        var success = await _removeLabelHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }
}

