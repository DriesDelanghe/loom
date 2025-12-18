namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record CreateWorkflowDefinitionRequest(
    Guid TenantId,
    string Name,
    string? Description
);

