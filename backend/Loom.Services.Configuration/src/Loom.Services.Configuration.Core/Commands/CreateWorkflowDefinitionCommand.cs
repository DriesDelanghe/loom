namespace Loom.Services.Configuration.Core.Commands;

public record CreateWorkflowDefinitionCommand(
    Guid TenantId,
    string Name,
    string? Description
);


