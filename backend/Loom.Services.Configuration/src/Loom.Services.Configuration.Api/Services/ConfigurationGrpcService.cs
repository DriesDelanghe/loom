using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Core.Queries.Handlers;
using ProtoBuf.Grpc;

namespace Loom.Services.Configuration.Api.Services;

public class ConfigurationGrpcService : IConfigurationService
{
    private readonly CreateWorkflowDefinitionCommandHandler _createWorkflowHandler;
    private readonly CreateDraftWorkflowVersionCommandHandler _createDraftHandler;
    private readonly PublishWorkflowVersionCommandHandler _publishHandler;
    private readonly DeleteWorkflowVersionCommandHandler _deleteVersionHandler;
    private readonly AddNodeCommandHandler _addNodeHandler;
    private readonly UpdateNodeConfigCommandHandler _updateNodeHandler;
    private readonly RemoveNodeCommandHandler _removeNodeHandler;
    private readonly AddConnectionCommandHandler _addConnectionHandler;
    private readonly RemoveConnectionCommandHandler _removeConnectionHandler;
    private readonly AddWorkflowVariableCommandHandler _addVariableHandler;
    private readonly UpdateWorkflowVariableCommandHandler _updateVariableHandler;
    private readonly RemoveWorkflowVariableCommandHandler _removeVariableHandler;
    private readonly AddWorkflowLabelDefinitionCommandHandler _addLabelHandler;
    private readonly RemoveWorkflowLabelDefinitionCommandHandler _removeLabelHandler;
    private readonly CreateTriggerCommandHandler _createTriggerHandler;
    private readonly UpdateTriggerConfigCommandHandler _updateTriggerHandler;
    private readonly DeleteTriggerCommandHandler _deleteTriggerHandler;
    private readonly BindTriggerToWorkflowVersionCommandHandler _bindHandler;
    private readonly UnbindTriggerFromWorkflowVersionCommandHandler _unbindHandler;
    private readonly GetWorkflowDefinitionsQueryHandler _getDefinitionsHandler;
    private readonly GetWorkflowVersionsQueryHandler _getVersionsHandler;
    private readonly GetWorkflowVersionDetailsQueryHandler _getVersionDetailsHandler;
    private readonly GetCompiledWorkflowVersionQueryHandler _getCompiledHandler;
    private readonly ValidateWorkflowVersionQueryHandler _validateHandler;
    private readonly GetWorkflowVersionsForTriggerQueryHandler _getVersionsForTriggerHandler;

    public ConfigurationGrpcService(
        CreateWorkflowDefinitionCommandHandler createWorkflowHandler,
        CreateDraftWorkflowVersionCommandHandler createDraftHandler,
        PublishWorkflowVersionCommandHandler publishHandler,
        DeleteWorkflowVersionCommandHandler deleteVersionHandler,
        AddNodeCommandHandler addNodeHandler,
        UpdateNodeConfigCommandHandler updateNodeHandler,
        RemoveNodeCommandHandler removeNodeHandler,
        AddConnectionCommandHandler addConnectionHandler,
        RemoveConnectionCommandHandler removeConnectionHandler,
        AddWorkflowVariableCommandHandler addVariableHandler,
        UpdateWorkflowVariableCommandHandler updateVariableHandler,
        RemoveWorkflowVariableCommandHandler removeVariableHandler,
        AddWorkflowLabelDefinitionCommandHandler addLabelHandler,
        RemoveWorkflowLabelDefinitionCommandHandler removeLabelHandler,
        CreateTriggerCommandHandler createTriggerHandler,
        UpdateTriggerConfigCommandHandler updateTriggerHandler,
        DeleteTriggerCommandHandler deleteTriggerHandler,
        BindTriggerToWorkflowVersionCommandHandler bindHandler,
        UnbindTriggerFromWorkflowVersionCommandHandler unbindHandler,
        GetWorkflowDefinitionsQueryHandler getDefinitionsHandler,
        GetWorkflowVersionsQueryHandler getVersionsHandler,
        GetWorkflowVersionDetailsQueryHandler getVersionDetailsHandler,
        GetCompiledWorkflowVersionQueryHandler getCompiledHandler,
        ValidateWorkflowVersionQueryHandler validateHandler,
        GetWorkflowVersionsForTriggerQueryHandler getVersionsForTriggerHandler)
    {
        _createWorkflowHandler = createWorkflowHandler;
        _createDraftHandler = createDraftHandler;
        _publishHandler = publishHandler;
        _deleteVersionHandler = deleteVersionHandler;
        _addNodeHandler = addNodeHandler;
        _updateNodeHandler = updateNodeHandler;
        _removeNodeHandler = removeNodeHandler;
        _addConnectionHandler = addConnectionHandler;
        _removeConnectionHandler = removeConnectionHandler;
        _addVariableHandler = addVariableHandler;
        _updateVariableHandler = updateVariableHandler;
        _removeVariableHandler = removeVariableHandler;
        _addLabelHandler = addLabelHandler;
        _removeLabelHandler = removeLabelHandler;
        _createTriggerHandler = createTriggerHandler;
        _updateTriggerHandler = updateTriggerHandler;
        _deleteTriggerHandler = deleteTriggerHandler;
        _bindHandler = bindHandler;
        _unbindHandler = unbindHandler;
        _getDefinitionsHandler = getDefinitionsHandler;
        _getVersionsHandler = getVersionsHandler;
        _getVersionDetailsHandler = getVersionDetailsHandler;
        _getCompiledHandler = getCompiledHandler;
        _validateHandler = validateHandler;
        _getVersionsForTriggerHandler = getVersionsForTriggerHandler;
    }

    public async Task<IdResponse> CreateWorkflowDefinition(
        CreateWorkflowDefinitionRequest request,
        CallContext context = default)
    {
        var command = new CreateWorkflowDefinitionCommand(
            Guid.Parse(request.TenantId),
            request.Name,
            request.Description
        );

        var id = await _createWorkflowHandler.HandleAsync(command, context.CancellationToken);
        return new IdResponse { Id = id.ToString() };
    }

    public async Task<IdResponse> CreateDraftWorkflowVersion(
        CreateDraftWorkflowVersionRequest request,
        CallContext context = default)
    {
        var command = new CreateDraftWorkflowVersionCommand(
            Guid.Parse(request.WorkflowDefinitionId),
            request.CreatedBy
        );

        var id = await _createDraftHandler.HandleAsync(command, context.CancellationToken);
        return new IdResponse { Id = id.ToString() };
    }

    public async Task<SuccessResponse> PublishWorkflowVersion(
        PublishWorkflowVersionRequest request,
        CallContext context = default)
    {
        var command = new PublishWorkflowVersionCommand(
            Guid.Parse(request.WorkflowVersionId),
            request.PublishedBy
        );

        var success = await _publishHandler.HandleAsync(command, context.CancellationToken);
        return new SuccessResponse { Success = success };
    }

    public async Task<SuccessResponse> DeleteWorkflowVersion(
        DeleteWorkflowVersionRequest request,
        CallContext context = default)
    {
        var command = new DeleteWorkflowVersionCommand(Guid.Parse(request.WorkflowVersionId));
        var success = await _deleteVersionHandler.HandleAsync(command, context.CancellationToken);
        return new SuccessResponse { Success = success };
    }

    public async Task<GetWorkflowDefinitionsResponse> GetWorkflowDefinitions(
        GetWorkflowDefinitionsRequest request,
        CallContext context = default)
    {
        var query = new GetWorkflowDefinitionsQuery(Guid.Parse(request.TenantId));
        var result = await _getDefinitionsHandler.HandleAsync(query, context.CancellationToken);

        var response = new GetWorkflowDefinitionsResponse();
        response.Definitions.AddRange(result.Select(d => new WorkflowDefinition
        {
            Id = d.Id.ToString(),
            Name = d.Name,
            HasPublishedVersion = d.HasPublishedVersion,
            LatestVersion = d.LatestVersion ?? 0
        }));

        return response;
    }

    public async Task<GetWorkflowVersionsResponse> GetWorkflowVersions(
        GetWorkflowVersionsRequest request,
        CallContext context = default)
    {
        var query = new GetWorkflowVersionsQuery(Guid.Parse(request.WorkflowDefinitionId));
        var result = await _getVersionsHandler.HandleAsync(query, context.CancellationToken);

        var response = new GetWorkflowVersionsResponse();
        response.Versions.AddRange(result.Select(v => new WorkflowVersion
        {
            Id = v.Id.ToString(),
            Version = v.Version,
            Status = v.Status,
            CreatedAt = v.CreatedAt,
            PublishedAt = v.PublishedAt
        }));

        return response;
    }

    public async Task<GetCompiledWorkflowResponse> GetCompiledWorkflow(
        GetCompiledWorkflowRequest request,
        CallContext context = default)
    {
        var query = new GetCompiledWorkflowVersionQuery(Guid.Parse(request.WorkflowVersionId));
        var result = await _getCompiledHandler.HandleAsync(query, context.CancellationToken);

        var response = new GetCompiledWorkflowResponse
        {
            Version = new WorkflowVersionInfo
            {
                Id = result.Version.Id.ToString(),
                DefinitionId = result.Version.DefinitionId.ToString(),
                Version = result.Version.Version,
                Status = result.Version.Status.ToString()
            }
        };

        response.Nodes.AddRange(result.Nodes.Select(n => new NodeInfo
        {
            Id = n.Id.ToString(),
            Key = n.Key,
            Name = n.Name ?? string.Empty,
            Type = n.Type.ToString()
        }));

        return response;
    }

    public async Task<ValidationResultResponse> ValidateWorkflowVersion(
        ValidateWorkflowVersionRequest request,
        CallContext context = default)
    {
        var query = new ValidateWorkflowVersionQuery(Guid.Parse(request.WorkflowVersionId));
        var result = await _validateHandler.HandleAsync(query, context.CancellationToken);

        var response = new ValidationResultResponse
        {
            IsValid = result.IsValid
        };
        response.Errors.AddRange(result.Errors);
        response.Warnings.AddRange(result.Warnings);

        return response;
    }
}

