using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Observability;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class UpdateWorkflowVariableCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_UpdatesVariable_InDraftVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new UpdateWorkflowVariableCommandHandler(dbContext);

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

        var variable = new WorkflowVariableEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "var-1",
            Type = VariableType.String,
            Description = "Old description"
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Variables.Add(variable);
        await dbContext.SaveChangesAsync();

        var command = new UpdateWorkflowVariableCommand(
            VariableId: variable.Id,
            Type: VariableType.Int,
            InitialValue: null,
            Description: "New description"
        );

        var result = await handler.HandleAsync(command);

        Assert.True(result);
        
        var updatedVariable = await dbContext.Variables.FindAsync(variable.Id);
        Assert.NotNull(updatedVariable);
        Assert.Equal(VariableType.Int, updatedVariable!.Type);
        Assert.Equal("New description", updatedVariable.Description);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNotDraft()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new UpdateWorkflowVariableCommandHandler(dbContext);

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
            Status = WorkflowStatus.Published,
            CreatedAt = DateTime.UtcNow
        };

        var variable = new WorkflowVariableEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "var-1",
            Type = VariableType.String
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Variables.Add(variable);
        await dbContext.SaveChangesAsync();

        var command = new UpdateWorkflowVariableCommand(
            VariableId: variable.Id,
            Type: null,
            InitialValue: null,
            Description: "New"
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

