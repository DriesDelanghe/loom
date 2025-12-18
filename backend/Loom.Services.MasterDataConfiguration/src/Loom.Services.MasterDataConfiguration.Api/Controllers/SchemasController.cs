using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;
using Loom.Services.MasterDataConfiguration.Core.Queries;
using Loom.Services.MasterDataConfiguration.Core.Queries.Handlers;
using Loom.Services.MasterDataConfiguration.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.MasterDataConfiguration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchemasController : ControllerBase
{
    private readonly CreateDataSchemaCommandHandler _createSchemaHandler;
    private readonly AddFieldDefinitionCommandHandler _addFieldHandler;
    private readonly RemoveFieldDefinitionCommandHandler _removeFieldHandler;
    private readonly UpdateFieldDefinitionCommandHandler _updateFieldHandler;
    private readonly PublishDataSchemaCommandHandler _publishSchemaHandler;
    private readonly GetSchemasQueryHandler _getSchemasHandler;
    private readonly GetSchemaDetailsQueryHandler _getSchemaDetailsHandler;
    private readonly GetSchemaGraphQueryHandler _getSchemaGraphHandler;
    private readonly ValidateSchemaQueryHandler _validateSchemaHandler;
    private readonly GetUnpublishedDependenciesQueryHandler _getUnpublishedDependenciesHandler;
    private readonly PublishRelatedSchemasCommandHandler _publishRelatedSchemasHandler;
    private readonly DeleteSchemaVersionCommandHandler _deleteSchemaVersionHandler;
    private readonly DeleteSchemaCommandHandler _deleteSchemaHandler;
    private readonly AddSchemaTagCommandHandler _addTagHandler;
    private readonly RemoveSchemaTagCommandHandler _removeTagHandler;
    private readonly RemoveSchemaTagByValueCommandHandler _removeTagByValueHandler;

    public SchemasController(
        CreateDataSchemaCommandHandler createSchemaHandler,
        AddFieldDefinitionCommandHandler addFieldHandler,
        RemoveFieldDefinitionCommandHandler removeFieldHandler,
        UpdateFieldDefinitionCommandHandler updateFieldHandler,
        PublishDataSchemaCommandHandler publishSchemaHandler,
        GetSchemasQueryHandler getSchemasHandler,
        GetSchemaDetailsQueryHandler getSchemaDetailsHandler,
        GetSchemaGraphQueryHandler getSchemaGraphHandler,
        ValidateSchemaQueryHandler validateSchemaHandler,
        GetUnpublishedDependenciesQueryHandler getUnpublishedDependenciesHandler,
        PublishRelatedSchemasCommandHandler publishRelatedSchemasHandler,
        DeleteSchemaVersionCommandHandler deleteSchemaVersionHandler,
        DeleteSchemaCommandHandler deleteSchemaHandler,
        AddSchemaTagCommandHandler addTagHandler,
        RemoveSchemaTagCommandHandler removeTagHandler,
        RemoveSchemaTagByValueCommandHandler removeTagByValueHandler)
    {
        _createSchemaHandler = createSchemaHandler;
        _addFieldHandler = addFieldHandler;
        _removeFieldHandler = removeFieldHandler;
        _updateFieldHandler = updateFieldHandler;
        _publishSchemaHandler = publishSchemaHandler;
        _getSchemasHandler = getSchemasHandler;
        _getSchemaDetailsHandler = getSchemaDetailsHandler;
        _getSchemaGraphHandler = getSchemaGraphHandler;
        _validateSchemaHandler = validateSchemaHandler;
        _getUnpublishedDependenciesHandler = getUnpublishedDependenciesHandler;
        _publishRelatedSchemasHandler = publishRelatedSchemasHandler;
        _deleteSchemaVersionHandler = deleteSchemaVersionHandler;
        _deleteSchemaHandler = deleteSchemaHandler;
        _addTagHandler = addTagHandler;
        _removeTagHandler = removeTagHandler;
        _removeTagByValueHandler = removeTagByValueHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DataSchemaSummary>>> GetSchemas(
        [FromQuery] Guid tenantId,
        [FromQuery] Domain.Schemas.SchemaRole? role,
        [FromQuery] Domain.Schemas.SchemaStatus? status,
        CancellationToken cancellationToken)
    {
        var query = new GetSchemasQuery(tenantId, role, status);
        var result = await _getSchemasHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DataSchemaDetails>> GetSchemaDetails(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetSchemaDetailsQuery(id);
        var result = await _getSchemaDetailsHandler.HandleAsync(query, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}/graph")]
    public async Task<ActionResult<SchemaGraph>> GetSchemaGraph(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetSchemaGraphQuery(id);
        var result = await _getSchemaGraphHandler.HandleAsync(query, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}/validate")]
    public async Task<ActionResult<ValidationResult>> ValidateSchema(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new ValidateSchemaQuery(id);
        var result = await _validateSchemaHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> CreateSchema(
        [FromBody] CreateDataSchemaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDataSchemaCommand(
            request.TenantId,
            request.DataModelId,
            request.Role,
            request.Key,
            request.Description
        );

        var id = await _createSchemaHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpPost("{id}/fields")]
    public async Task<ActionResult<IdResponse>> AddField(
        Guid id,
        [FromBody] AddFieldDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddFieldDefinitionCommand(
            id,
            request.Path,
            request.FieldType,
            request.ScalarType,
            request.ElementSchemaId,
            request.Required,
            request.Description
        );

        var fieldId = await _addFieldHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(fieldId));
    }

    [HttpDelete("fields/{fieldId}")]
    public async Task<ActionResult<SuccessResponse>> RemoveField(
        Guid fieldId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveFieldDefinitionCommand(fieldId);
        await _removeFieldHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPut("fields/{fieldId}")]
    public async Task<ActionResult<SuccessResponse>> UpdateField(
        Guid fieldId,
        [FromBody] UpdateFieldDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateFieldDefinitionCommand(
            fieldId,
            request.Path,
            request.FieldType,
            request.ScalarType,
            request.ElementSchemaId,
            request.Required,
            request.Description
        );
        await _updateFieldHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPost("{id}/publish")]
    public async Task<ActionResult<SuccessResponse>> PublishSchema(
        Guid id,
        [FromBody] PublishRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PublishDataSchemaCommand(id, request.PublishedBy);
        await _publishSchemaHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<SuccessResponse>> DeleteSchemaVersion(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSchemaVersionCommand(id);
        await _deleteSchemaVersionHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpDelete]
    public async Task<ActionResult<SuccessResponse>> DeleteSchema(
        [FromQuery] string key,
        [FromQuery] Domain.Schemas.SchemaRole role,
        [FromQuery] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSchemaCommand(key, role, tenantId);
        await _deleteSchemaHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPost("{id}/tags")]
    public async Task<ActionResult<IdResponse>> AddTag(
        Guid id,
        [FromBody] AddTagRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddSchemaTagCommand(id, request.Tag);
        var tagId = await _addTagHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(tagId));
    }

    [HttpDelete("tags/{tagId}")]
    public async Task<ActionResult<SuccessResponse>> RemoveTag(
        Guid tagId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveSchemaTagCommand(tagId);
        await _removeTagHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpDelete("{id}/tags")]
    public async Task<ActionResult<SuccessResponse>> RemoveTagByValue(
        Guid id,
        [FromQuery] string tag,
        CancellationToken cancellationToken)
    {
        var command = new RemoveSchemaTagByValueCommand(id, tag);
        await _removeTagByValueHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }
}

public class CreateDataSchemaRequest
{
    public Guid TenantId { get; set; }
    public Guid? DataModelId { get; set; }
    public Domain.Schemas.SchemaRole Role { get; set; }
    public string Key { get; set; } = default!;
    public string? Description { get; set; }
}

public class AddFieldDefinitionRequest
{
    public string Path { get; set; } = default!;
    public Domain.Schemas.FieldType FieldType { get; set; }
    public Domain.Schemas.ScalarType? ScalarType { get; set; }
    public Guid? ElementSchemaId { get; set; }
    public bool Required { get; set; }
    public string? Description { get; set; }
}

public class UpdateFieldDefinitionRequest
{
    public string? Path { get; set; }
    public Domain.Schemas.FieldType? FieldType { get; set; }
    public Domain.Schemas.ScalarType? ScalarType { get; set; }
    public Guid? ElementSchemaId { get; set; }
    public bool? Required { get; set; }
    public string? Description { get; set; }
}

public class PublishRequest
{
    public string PublishedBy { get; set; } = default!;
}

public class AddTagRequest
{
    public string Tag { get; set; } = default!;
}

public class IdResponse
{
    public Guid Id { get; set; }

    public IdResponse(Guid id)
    {
        Id = id;
    }
}

public class SuccessResponse
{
    public bool Success { get; set; }

    public SuccessResponse(bool success)
    {
        Success = success;
    }
}

public class UnpublishedDependencyDto
{
    public Guid SchemaId { get; set; }
    public string Key { get; set; } = default!;
    public int Version { get; set; }
    public Domain.Schemas.SchemaStatus Status { get; set; }
    public Domain.Schemas.SchemaRole Role { get; set; }
}

public class PublishRelatedSchemasRequest
{
    public string PublishedBy { get; set; } = default!;
    public IReadOnlyList<Guid> RelatedSchemaIds { get; set; } = Array.Empty<Guid>();
}

public class PublishRelatedSchemasResponse
{
    public IReadOnlyList<Guid> PublishedSchemaIds { get; set; } = Array.Empty<Guid>();
}

