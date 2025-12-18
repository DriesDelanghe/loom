using Loom.Services.MasterDataConfiguration.Core.Commands;
using Loom.Services.MasterDataConfiguration.Core.Commands.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Loom.Services.MasterDataConfiguration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataModelsController : ControllerBase
{
    private readonly CreateDataModelCommandHandler _createModelHandler;

    public DataModelsController(CreateDataModelCommandHandler createModelHandler)
    {
        _createModelHandler = createModelHandler;
    }

    [HttpPost]
    public async Task<ActionResult<IdResponse>> CreateDataModel(
        [FromBody] CreateDataModelRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDataModelCommand(
            request.TenantId,
            request.Key,
            request.Name,
            request.Description
        );

        var id = await _createModelHandler.HandleAsync(command, cancellationToken);
        return Ok(new IdResponse(id));
    }
}

public class CreateDataModelRequest
{
    public Guid TenantId { get; set; }
    public string Key { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
