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
public class WorkflowsController : ControllerBase
{
    private readonly CreateWorkflowDefinitionCommandHandler _createWorkflowHandler;
    private readonly CreateDraftWorkflowVersionCommandHandler _createDraftHandler;
    private readonly PublishWorkflowVersionCommandHandler _publishHandler;
    private readonly DeleteWorkflowVersionCommandHandler _deleteVersionHandler;
    private readonly GetWorkflowDefinitionsQueryHandler _getDefinitionsHandler;
    private readonly GetWorkflowVersionsQueryHandler _getVersionsHandler;
    private readonly GetWorkflowVersionDetailsQueryHandler _getVersionDetailsHandler;
    private readonly GetCompiledWorkflowVersionQueryHandler _getCompiledHandler;
    private readonly ValidateWorkflowVersionQueryHandler _validateHandler;

    public WorkflowsController(
        CreateWorkflowDefinitionCommandHandler createWorkflowHandler,
        CreateDraftWorkflowVersionCommandHandler createDraftHandler,
        PublishWorkflowVersionCommandHandler publishHandler,
        DeleteWorkflowVersionCommandHandler deleteVersionHandler,
        GetWorkflowDefinitionsQueryHandler getDefinitionsHandler,
        GetWorkflowVersionsQueryHandler getVersionsHandler,
        GetWorkflowVersionDetailsQueryHandler getVersionDetailsHandler,
        GetCompiledWorkflowVersionQueryHandler getCompiledHandler,
        ValidateWorkflowVersionQueryHandler validateHandler)
    {
        _createWorkflowHandler = createWorkflowHandler;
        _createDraftHandler = createDraftHandler;
        _publishHandler = publishHandler;
        _deleteVersionHandler = deleteVersionHandler;
        _getDefinitionsHandler = getDefinitionsHandler;
        _getVersionsHandler = getVersionsHandler;
        _getVersionDetailsHandler = getVersionDetailsHandler;
        _getCompiledHandler = getCompiledHandler;
        _validateHandler = validateHandler;
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> CreateWorkflowDefinition(
        [FromBody] CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWorkflowDefinitionCommand(
            request.TenantId,
            request.Name,
            request.Description
        );

        var id = await _createWorkflowHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpPost("{workflowDefinitionId}/versions/draft")]
    public async Task<ActionResult<IdResponse>> CreateDraftVersion(
        Guid workflowDefinitionId,
        [FromBody] CreateDraftWorkflowVersionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDraftWorkflowVersionCommand(
            workflowDefinitionId,
            request.CreatedBy
        );

        var id = await _createDraftHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpPost("versions/{workflowVersionId}/publish")]
    public async Task<ActionResult<SuccessResponse>> PublishVersion(
        Guid workflowVersionId,
        [FromBody] PublishWorkflowVersionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PublishWorkflowVersionCommand(
            workflowVersionId,
            request.PublishedBy
        );

        var success = await _publishHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpDelete("versions/{workflowVersionId}")]
    public async Task<ActionResult<SuccessResponse>> DeleteWorkflowVersion(
        Guid workflowVersionId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteWorkflowVersionCommand(workflowVersionId);
        var success = await _deleteVersionHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(success));
    }

    [HttpGet]
    public async Task<ActionResult<List<WorkflowDefinitionResponse>>> GetWorkflowDefinitions(
        [FromQuery] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var query = new GetWorkflowDefinitionsQuery(tenantId);
        var result = await _getDefinitionsHandler.HandleAsync(query, cancellationToken);

        var response = result.Select(d => new WorkflowDefinitionResponse(
            d.Id,
            d.Name,
            d.HasPublishedVersion,
            d.LatestVersion
        )).ToList();

        return Ok(response);
    }

    [HttpGet("{workflowDefinitionId}/versions")]
    public async Task<ActionResult<List<WorkflowVersionResponse>>> GetWorkflowVersions(
        Guid workflowDefinitionId,
        CancellationToken cancellationToken)
    {
        var query = new GetWorkflowVersionsQuery(workflowDefinitionId);
        var result = await _getVersionsHandler.HandleAsync(query, cancellationToken);

        var response = result.Select(v => new WorkflowVersionResponse(
            v.Id,
            v.Version,
            v.Status,
            v.CreatedAt,
            v.PublishedAt
        )).ToList();

        return Ok(response);
    }

    [HttpGet("versions/{workflowVersionId}")]
    public async Task<ActionResult<WorkflowVersionDetailsResponse>> GetWorkflowVersionDetails(
        Guid workflowVersionId,
        CancellationToken cancellationToken)
    {
        var query = new GetWorkflowVersionDetailsQuery(workflowVersionId);
        var result = await _getVersionDetailsHandler.HandleAsync(query, cancellationToken);

        var triggerBindings = result.TriggerBindings.Select(tb => new TriggerBindingResponse(
            tb.Id,
            tb.TriggerId,
            tb.WorkflowVersionId,
            tb.Enabled,
            tb.Priority,
            tb.NodeBindings.Select(nb => new TriggerNodeBindingResponse(nb.Id, nb.EntryNodeId, nb.Order)).ToList(),
            tb.TriggerType,
            tb.TriggerConfig
        )).ToList();

        var response = new WorkflowVersionDetailsResponse(
            result.Version,
            result.Nodes,
            result.Connections,
            result.Variables,
            result.Labels,
            result.Settings,
            triggerBindings
        );

        return Ok(response);
    }

    [HttpGet("versions/{workflowVersionId}/compiled")]
    public async Task<ActionResult<CompiledWorkflowResponse>> GetCompiledWorkflow(
        Guid workflowVersionId,
        CancellationToken cancellationToken)
    {
        var query = new GetCompiledWorkflowVersionQuery(workflowVersionId);
        var result = await _getCompiledHandler.HandleAsync(query, cancellationToken);

        var triggers = result.Triggers.Select(t => new CompiledTriggerResponse(
            t.TriggerId,
            t.Type,
            t.Config,
            t.EntryNodeIds
        )).ToList();

        var response = new CompiledWorkflowResponse(
            result.Version,
            result.Nodes,
            result.Connections,
            result.Variables,
            result.Labels,
            result.Settings,
            triggers
        );

        return Ok(response);
    }

    [HttpGet("versions/{workflowVersionId}/validate")]
    public async Task<ActionResult<ValidationResultResponse>> ValidateWorkflowVersion(
        Guid workflowVersionId,
        CancellationToken cancellationToken)
    {
        var query = new ValidateWorkflowVersionQuery(workflowVersionId);
        var result = await _validateHandler.HandleAsync(query, cancellationToken);

        var response = new ValidationResultResponse(
            result.IsValid,
            result.Errors,
            result.Warnings
        );

        return Ok(response);
    }
}

