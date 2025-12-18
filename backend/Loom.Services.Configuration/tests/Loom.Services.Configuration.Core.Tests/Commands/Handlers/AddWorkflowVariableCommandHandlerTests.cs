using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Observability;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class AddWorkflowVariableCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsVariable_ToDraftVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new AddWorkflowVariableCommandHandler(dbContext);

        var definition = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var version = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Version = 1,
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        await dbContext.SaveChangesAsync();

        var command = new AddWorkflowVariableCommand(
            WorkflowVersionId: version.Id,
            Key: "var-1",
            Type: VariableType.String,
            InitialValue: null,
            Description: "Test variable"
        );

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);
        
        var variable = await dbContext.Variables.FindAsync(result);
        Assert.NotNull(variable);
        Assert.Equal(version.Id, variable!.WorkflowVersionId);
        Assert.Equal("var-1", variable.Key);
        Assert.Equal(VariableType.String, variable.Type);
        Assert.Equal("Test variable", variable.Description);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenKeyExists()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new AddWorkflowVariableCommandHandler(dbContext);

        var definition = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var version = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Version = 1,
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var existingVariable = new WorkflowVariableEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "var-1",
            Type = VariableType.String
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Variables.Add(existingVariable);
        await dbContext.SaveChangesAsync();

        var command = new AddWorkflowVariableCommand(
            WorkflowVersionId: version.Id,
            Key: "var-1",
            Type: VariableType.Int,
            InitialValue: null,
            Description: null
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

