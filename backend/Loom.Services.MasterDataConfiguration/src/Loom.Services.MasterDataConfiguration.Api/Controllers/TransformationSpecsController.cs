using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;
using Loom.Services.MasterDataConfiguration.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.MasterDataConfiguration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransformationSpecsController : ControllerBase
{
    private readonly CreateTransformationSpecCommandHandler _createSpecHandler;
    private readonly AddSimpleTransformRuleCommandHandler _addSimpleRuleHandler;
    private readonly RemoveSimpleTransformRuleCommandHandler _removeSimpleRuleHandler;
    private readonly UpdateSimpleTransformRuleCommandHandler _updateSimpleRuleHandler;
    private readonly AddTransformReferenceCommandHandler _addReferenceHandler;
    private readonly AddTransformGraphNodeCommandHandler _addGraphNodeHandler;
    private readonly AddTransformGraphEdgeCommandHandler _addGraphEdgeHandler;
    private readonly AddTransformOutputBindingCommandHandler _addOutputBindingHandler;
    private readonly RemoveTransformGraphNodeCommandHandler _removeGraphNodeHandler;
    private readonly RemoveTransformGraphEdgeCommandHandler _removeGraphEdgeHandler;
    private readonly PublishTransformationSpecCommandHandler _publishSpecHandler;
    private readonly GetTransformationSpecDetailsQueryHandler _getSpecDetailsHandler;
    private readonly GetTransformationSpecBySourceSchemaIdQueryHandler _getSpecBySourceSchemaIdHandler;
    private readonly ValidateTransformationSpecQueryHandler _validateSpecHandler;
    private readonly GetCompiledTransformationSpecQueryHandler _getCompiledSpecHandler;

    public TransformationSpecsController(
        CreateTransformationSpecCommandHandler createSpecHandler,
        AddSimpleTransformRuleCommandHandler addSimpleRuleHandler,
        RemoveSimpleTransformRuleCommandHandler removeSimpleRuleHandler,
        UpdateSimpleTransformRuleCommandHandler updateSimpleRuleHandler,
        AddTransformReferenceCommandHandler addReferenceHandler,
        AddTransformGraphNodeCommandHandler addGraphNodeHandler,
        AddTransformGraphEdgeCommandHandler addGraphEdgeHandler,
        AddTransformOutputBindingCommandHandler addOutputBindingHandler,
        RemoveTransformGraphNodeCommandHandler removeGraphNodeHandler,
        RemoveTransformGraphEdgeCommandHandler removeGraphEdgeHandler,
        PublishTransformationSpecCommandHandler publishSpecHandler,
        GetTransformationSpecDetailsQueryHandler getSpecDetailsHandler,
        GetTransformationSpecBySourceSchemaIdQueryHandler getSpecBySourceSchemaIdHandler,
        ValidateTransformationSpecQueryHandler validateSpecHandler,
        GetCompiledTransformationSpecQueryHandler getCompiledSpecHandler)
    {
        _createSpecHandler = createSpecHandler;
        _addSimpleRuleHandler = addSimpleRuleHandler;
        _removeSimpleRuleHandler = removeSimpleRuleHandler;
        _updateSimpleRuleHandler = updateSimpleRuleHandler;
        _addReferenceHandler = addReferenceHandler;
        _addGraphNodeHandler = addGraphNodeHandler;
        _addGraphEdgeHandler = addGraphEdgeHandler;
        _addOutputBindingHandler = addOutputBindingHandler;
        _removeGraphNodeHandler = removeGraphNodeHandler;
        _removeGraphEdgeHandler = removeGraphEdgeHandler;
        _publishSpecHandler = publishSpecHandler;
        _getSpecDetailsHandler = getSpecDetailsHandler;
        _getSpecBySourceSchemaIdHandler = getSpecBySourceSchemaIdHandler;
        _validateSpecHandler = validateSpecHandler;
        _getCompiledSpecHandler = getCompiledSpecHandler;
    }

    [HttpGet("by-source-schema/{sourceSchemaId}")]
    public async Task<ActionResult<TransformationSpecDetails>> GetTransformationSpecBySourceSchemaId(
        Guid sourceSchemaId,
        CancellationToken cancellationToken)
    {
        var query = new GetTransformationSpecBySourceSchemaIdQuery(sourceSchemaId);
        var result = await _getSpecBySourceSchemaIdHandler.HandleAsync(query, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransformationSpecDetails>> GetTransformationSpecDetails(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTransformationSpecDetailsQuery(id);
        var result = await _getSpecDetailsHandler.HandleAsync(query, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}/compiled")]
    public async Task<ActionResult<CompiledTransformationSpec>> GetCompiledTransformationSpec(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCompiledTransformationSpecQuery(id);
        var result = await _getCompiledSpecHandler.HandleAsync(query, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost("{id}/validate")]
    public async Task<ActionResult<ValidationResult>> ValidateTransformationSpec(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new ValidateTransformationSpecQuery(id);
        var result = await _validateSpecHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> CreateTransformationSpec(
        [FromBody] CreateTransformationSpecRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTransformationSpecCommand(
            request.TenantId,
            request.SourceSchemaId,
            request.TargetSchemaId,
            request.Mode,
            request.Cardinality,
            request.Description
        );

        var id = await _createSpecHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpPost("{id}/simple-rules")]
    public async Task<ActionResult<IdResponse>> AddSimpleTransformRule(
        Guid id,
        [FromBody] AddSimpleTransformRuleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddSimpleTransformRuleCommand(
            id,
            request.SourcePath,
            request.TargetPath,
            request.ConverterId,
            request.Required,
            request.Order
        );

        var ruleId = await _addSimpleRuleHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(ruleId));
    }

    [HttpDelete("simple-rules/{ruleId}")]
    public async Task<ActionResult<SuccessResponse>> RemoveSimpleTransformRule(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveSimpleTransformRuleCommand(ruleId);
        await _removeSimpleRuleHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPut("simple-rules/{ruleId}")]
    public async Task<ActionResult<SuccessResponse>> UpdateSimpleTransformRule(
        Guid ruleId,
        [FromBody] UpdateSimpleTransformRuleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSimpleTransformRuleCommand(
            ruleId,
            request.SourcePath,
            request.TargetPath,
            request.ConverterId,
            request.Required
        );
        await _updateSimpleRuleHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPost("{id}/references")]
    public async Task<ActionResult<IdResponse>> AddTransformReference(
        Guid id,
        [FromBody] AddTransformReferenceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTransformReferenceCommand(
            id,
            request.SourceFieldPath,
            request.TargetFieldPath,
            request.ChildTransformationSpecId
        );

        var referenceId = await _addReferenceHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(referenceId));
    }

    [HttpPost("{id}/publish")]
    public async Task<ActionResult<SuccessResponse>> PublishTransformationSpec(
        Guid id,
        [FromBody] PublishRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PublishTransformationSpecCommand(id, request.PublishedBy);
        await _publishSpecHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPost("{id}/graph-nodes")]
    public async Task<ActionResult<IdResponse>> AddTransformGraphNode(
        Guid id,
        [FromBody] AddTransformGraphNodeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTransformGraphNodeCommand(
            id,
            request.Key,
            request.NodeType,
            request.OutputType,
            request.Config
        );

        var nodeId = await _addGraphNodeHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(nodeId));
    }

    [HttpDelete("graph-nodes/{nodeId}")]
    public async Task<ActionResult<SuccessResponse>> RemoveTransformGraphNode(
        Guid nodeId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveTransformGraphNodeCommand(nodeId);
        await _removeGraphNodeHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPost("{id}/graph-edges")]
    public async Task<ActionResult<IdResponse>> AddTransformGraphEdge(
        Guid id,
        [FromBody] AddTransformGraphEdgeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTransformGraphEdgeCommand(
            id,
            request.FromNodeId,
            request.ToNodeId,
            request.InputName,
            request.Order
        );

        var edgeId = await _addGraphEdgeHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(edgeId));
    }

    [HttpDelete("graph-edges/{edgeId}")]
    public async Task<ActionResult<SuccessResponse>> RemoveTransformGraphEdge(
        Guid edgeId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveTransformGraphEdgeCommand(edgeId);
        await _removeGraphEdgeHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPost("{id}/output-bindings")]
    public async Task<ActionResult<IdResponse>> AddTransformOutputBinding(
        Guid id,
        [FromBody] AddTransformOutputBindingRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTransformOutputBindingCommand(
            id,
            request.TargetPath,
            request.FromNodeId
        );

        var bindingId = await _addOutputBindingHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(bindingId));
    }
}

public class AddTransformGraphNodeRequest
{
    public string Key { get; set; } = default!;
    public Domain.Transformation.TransformNodeType NodeType { get; set; }
    public string OutputType { get; set; } = default!;
    public string Config { get; set; } = default!;
}

public class AddTransformGraphEdgeRequest
{
    public Guid FromNodeId { get; set; }
    public Guid ToNodeId { get; set; }
    public string InputName { get; set; } = default!;
    public int Order { get; set; }
}

public class AddTransformOutputBindingRequest
{
    public string TargetPath { get; set; } = default!;
    public Guid FromNodeId { get; set; }
}

public class CreateTransformationSpecRequest
{
    public Guid TenantId { get; set; }
    public Guid SourceSchemaId { get; set; }
    public Guid TargetSchemaId { get; set; }
    public Domain.Transformation.TransformationMode Mode { get; set; }
    public Domain.Transformation.Cardinality Cardinality { get; set; }
    public string? Description { get; set; }
}

public class AddSimpleTransformRuleRequest
{
    public string SourcePath { get; set; } = default!;
    public string TargetPath { get; set; } = default!;
    public Guid? ConverterId { get; set; }
    public bool Required { get; set; }
    public int Order { get; set; }
}

public class UpdateSimpleTransformRuleRequest
{
    public string? SourcePath { get; set; }
    public string? TargetPath { get; set; }
    public Guid? ConverterId { get; set; }
    public bool? Required { get; set; }
}

public class AddTransformReferenceRequest
{
    public string SourceFieldPath { get; set; } = default!;
    public string TargetFieldPath { get; set; } = default!;
    public Guid ChildTransformationSpecId { get; set; }
}

