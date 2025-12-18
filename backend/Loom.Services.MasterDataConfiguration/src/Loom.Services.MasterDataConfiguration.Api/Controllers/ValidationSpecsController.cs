using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;
using Loom.Services.MasterDataConfiguration.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.MasterDataConfiguration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidationSpecsController : ControllerBase
{
    private readonly CreateValidationSpecCommandHandler _createSpecHandler;
    private readonly AddValidationRuleCommandHandler _addRuleHandler;
    private readonly RemoveValidationRuleCommandHandler _removeRuleHandler;
    private readonly UpdateValidationRuleCommandHandler _updateRuleHandler;
    private readonly AddValidationReferenceCommandHandler _addReferenceHandler;
    private readonly PublishValidationSpecCommandHandler _publishSpecHandler;
    private readonly GetValidationSpecDetailsQueryHandler _getSpecDetailsHandler;
    private readonly GetValidationSpecBySchemaIdQueryHandler _getSpecBySchemaIdHandler;
    private readonly ValidateValidationSpecQueryHandler _validateSpecHandler;

    public ValidationSpecsController(
        CreateValidationSpecCommandHandler createSpecHandler,
        AddValidationRuleCommandHandler addRuleHandler,
        RemoveValidationRuleCommandHandler removeRuleHandler,
        UpdateValidationRuleCommandHandler updateRuleHandler,
        AddValidationReferenceCommandHandler addReferenceHandler,
        PublishValidationSpecCommandHandler publishSpecHandler,
        GetValidationSpecDetailsQueryHandler getSpecDetailsHandler,
        GetValidationSpecBySchemaIdQueryHandler getSpecBySchemaIdHandler,
        ValidateValidationSpecQueryHandler validateSpecHandler)
    {
        _createSpecHandler = createSpecHandler;
        _addRuleHandler = addRuleHandler;
        _removeRuleHandler = removeRuleHandler;
        _updateRuleHandler = updateRuleHandler;
        _addReferenceHandler = addReferenceHandler;
        _publishSpecHandler = publishSpecHandler;
        _getSpecDetailsHandler = getSpecDetailsHandler;
        _getSpecBySchemaIdHandler = getSpecBySchemaIdHandler;
        _validateSpecHandler = validateSpecHandler;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ValidationSpecDetails>> GetValidationSpecDetails(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetValidationSpecDetailsQuery(id);
        var result = await _getSpecDetailsHandler.HandleAsync(query, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("by-schema/{schemaId}")]
    public async Task<ActionResult<ValidationSpecDetails>> GetValidationSpecBySchemaId(
        Guid schemaId,
        CancellationToken cancellationToken)
    {
        var query = new GetValidationSpecBySchemaIdQuery(schemaId);
        var result = await _getSpecBySchemaIdHandler.HandleAsync(query, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost("{id}/validate")]
    public async Task<ActionResult<ValidationResult>> ValidateValidationSpec(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new ValidateValidationSpecQuery(id);
        var result = await _validateSpecHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> CreateValidationSpec(
        [FromBody] CreateValidationSpecRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateValidationSpecCommand(
            request.TenantId,
            request.DataSchemaId,
            request.Description
        );

        var id = await _createSpecHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpPost("{id}/rules")]
    public async Task<ActionResult<IdResponse>> AddValidationRule(
        Guid id,
        [FromBody] AddValidationRuleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddValidationRuleCommand(
            id,
            request.RuleType,
            request.Severity,
            request.Parameters
        );

        var ruleId = await _addRuleHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(ruleId));
    }

    [HttpDelete("rules/{ruleId}")]
    public async Task<ActionResult<SuccessResponse>> RemoveValidationRule(
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveValidationRuleCommand(ruleId);
        await _removeRuleHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPut("rules/{ruleId}")]
    public async Task<ActionResult<SuccessResponse>> UpdateValidationRule(
        Guid ruleId,
        [FromBody] UpdateValidationRuleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateValidationRuleCommand(
            ruleId,
            request.RuleType,
            request.Severity,
            request.Parameters
        );
        await _updateRuleHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPost("{id}/references")]
    public async Task<ActionResult<IdResponse>> AddValidationReference(
        Guid id,
        [FromBody] AddValidationReferenceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddValidationReferenceCommand(
            id,
            request.FieldPath,
            request.ChildValidationSpecId
        );

        var referenceId = await _addReferenceHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(referenceId));
    }

    [HttpPost("{id}/publish")]
    public async Task<ActionResult<SuccessResponse>> PublishValidationSpec(
        Guid id,
        [FromBody] PublishRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PublishValidationSpecCommand(id, request.PublishedBy);
        await _publishSpecHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }
}

public class CreateValidationSpecRequest
{
    public Guid TenantId { get; set; }
    public Guid DataSchemaId { get; set; }
    public string? Description { get; set; }
}

public class AddValidationRuleRequest
{
    public Domain.Validation.RuleType RuleType { get; set; }
    public Domain.Validation.Severity Severity { get; set; }
    public string Parameters { get; set; } = default!;
}

public class AddValidationReferenceRequest
{
    public string FieldPath { get; set; } = default!;
    public Guid ChildValidationSpecId { get; set; }
}

public class UpdateValidationRuleRequest
{
    public Domain.Validation.RuleType? RuleType { get; set; }
    public Domain.Validation.Severity? Severity { get; set; }
    public string? Parameters { get; set; }
}

