using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Loom.Services.MasterDataConfiguration.Core;
using Loom.Services.MasterDataConfiguration.Domain.Persistence;

namespace Loom.Services.MasterDataConfiguration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KeyDefinitionsController : ControllerBase
{
    private readonly AddKeyDefinitionCommandHandler _addKeyDefinitionHandler;
    private readonly RemoveKeyDefinitionCommandHandler _removeKeyDefinitionHandler;
    private readonly AddKeyFieldCommandHandler _addKeyFieldHandler;
    private readonly RemoveKeyFieldCommandHandler _removeKeyFieldHandler;
    private readonly ReorderKeyFieldsCommandHandler _reorderKeyFieldsHandler;
    private readonly MasterDataConfigurationDbContext _dbContext;

    public KeyDefinitionsController(
        AddKeyDefinitionCommandHandler addKeyDefinitionHandler,
        RemoveKeyDefinitionCommandHandler removeKeyDefinitionHandler,
        AddKeyFieldCommandHandler addKeyFieldHandler,
        RemoveKeyFieldCommandHandler removeKeyFieldHandler,
        ReorderKeyFieldsCommandHandler reorderKeyFieldsHandler,
        MasterDataConfigurationDbContext dbContext)
    {
        _addKeyDefinitionHandler = addKeyDefinitionHandler;
        _removeKeyDefinitionHandler = removeKeyDefinitionHandler;
        _addKeyFieldHandler = addKeyFieldHandler;
        _removeKeyFieldHandler = removeKeyFieldHandler;
        _reorderKeyFieldsHandler = reorderKeyFieldsHandler;
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> AddKeyDefinition(
        [FromBody] AddKeyDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddKeyDefinitionCommand(
            request.TenantId,
            request.DataSchemaId,
            request.Name,
            request.IsPrimary
        );

        var id = await _addKeyDefinitionHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<SuccessResponse>> RemoveKeyDefinition(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new RemoveKeyDefinitionCommand(id);
        await _removeKeyDefinitionHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPost("{id}/fields")]
    public async Task<ActionResult<IdResponse>> AddKeyField(
        Guid id,
        [FromBody] AddKeyFieldRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddKeyFieldCommand(
            id,
            request.FieldPath,
            request.Order,
            request.Normalization
        );

        var fieldId = await _addKeyFieldHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(fieldId));
    }

    [HttpDelete("fields/{fieldId}")]
    public async Task<ActionResult<SuccessResponse>> RemoveKeyField(
        Guid fieldId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveKeyFieldCommand(fieldId);
        await _removeKeyFieldHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpPut("{id}/fields/reorder")]
    public async Task<ActionResult<SuccessResponse>> ReorderKeyFields(
        Guid id,
        [FromBody] ReorderKeyFieldsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderKeyFieldsCommand(id, request.KeyFieldIdsInOrder);
        await _reorderKeyFieldsHandler.HandleAsync(command, cancellationToken);
        return Ok(new SuccessResponse(true));
    }

    [HttpGet("schemas/{schemaId}")]
    public async Task<ActionResult<BusinessKeysResponse>> GetSchemaBusinessKeys(
        Guid schemaId,
        CancellationToken cancellationToken)
    {
        var schema = await _dbContext.DataSchemas
            .Include(s => s.KeyDefinitions)
            .ThenInclude(k => k.KeyFields)
            .FirstOrDefaultAsync(s => s.Id == schemaId, cancellationToken);

        if (schema == null)
            return NotFound();

        var keys = schema.KeyDefinitions.Select(k => new BusinessKeyDto
        {
            Id = k.Id,
            Name = k.Name,
            IsPrimary = k.IsPrimary,
            CreatedAt = k.CreatedAt,
            Fields = k.KeyFields.OrderBy(f => f.Order).Select(f => new BusinessKeyFieldDto
            {
                Id = f.Id,
                FieldPath = f.FieldPath,
                Order = f.Order,
                Normalization = f.Normalization
            }).ToList()
        }).ToList();

        return Ok(new BusinessKeysResponse { Keys = keys });
    }
}

public class AddKeyDefinitionRequest
{
    public Guid TenantId { get; set; }
    public Guid DataSchemaId { get; set; }
    public string Name { get; set; } = default!;
    public bool IsPrimary { get; set; }
}

public class AddKeyFieldRequest
{
    public string FieldPath { get; set; } = default!;
    public int Order { get; set; }
    public string? Normalization { get; set; }
}

public class ReorderKeyFieldsRequest
{
    public IReadOnlyList<Guid> KeyFieldIdsInOrder { get; set; } = Array.Empty<Guid>();
}

public class BusinessKeysResponse
{
    public IReadOnlyList<BusinessKeyDto> Keys { get; set; } = Array.Empty<BusinessKeyDto>();
}

public class BusinessKeyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<BusinessKeyFieldDto> Fields { get; set; } = Array.Empty<BusinessKeyFieldDto>();
}

public class BusinessKeyFieldDto
{
    public Guid Id { get; set; }
    public string FieldPath { get; set; } = default!;
    public int Order { get; set; }
    public string? Normalization { get; set; }
}

