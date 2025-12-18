using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class CreateWorkflowDefinitionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesWorkflowDefinition_WithInitialDraftVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new CreateWorkflowDefinitionCommandHandler(dbContext);

        var command = new CreateWorkflowDefinitionCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Workflow",
            Description: "Test Description"
        );

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);
        
        var definition = await dbContext.WorkflowDefinitions.FindAsync(result);
        Assert.NotNull(definition);
        Assert.Equal(command.TenantId, definition!.TenantId);
        Assert.Equal(command.Name, definition.Name);
        Assert.Equal(command.Description, definition.Description);
        Assert.False(definition.IsArchived);

        var version = await dbContext.WorkflowVersions
            .FirstOrDefaultAsync(v => v.DefinitionId == result);
        
        Assert.NotNull(version);
        Assert.Equal(1, version!.Version);
        Assert.Equal(Domain.Workflows.WorkflowStatus.Draft, version.Status);
    }

    [Fact]
    public async Task HandleAsync_CreatesWorkflowDefinition_WithoutDescription()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new CreateWorkflowDefinitionCommandHandler(dbContext);

        var command = new CreateWorkflowDefinitionCommand(
            TenantId: Guid.NewGuid(),
            Name: "Test Workflow",
            Description: null
        );

        var result = await handler.HandleAsync(command);

        var definition = await dbContext.WorkflowDefinitions.FindAsync(result);
        Assert.NotNull(definition);
        Assert.Null(definition!.Description);
    }
}

