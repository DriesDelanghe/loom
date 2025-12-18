using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Observability;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class AddWorkflowLabelDefinitionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsLabel_ToDraftVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new AddWorkflowLabelDefinitionCommandHandler(dbContext);

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

        var command = new AddWorkflowLabelDefinitionCommand(
            WorkflowVersionId: version.Id,
            Key: "label-1",
            Type: LabelType.String,
            Description: "Test label"
        );

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);
        
        var label = await dbContext.Labels.FindAsync(result);
        Assert.NotNull(label);
        Assert.Equal(version.Id, label!.WorkflowVersionId);
        Assert.Equal("label-1", label.Key);
        Assert.Equal(LabelType.String, label.Type);
        Assert.Equal("Test label", label.Description);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenKeyExists()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new AddWorkflowLabelDefinitionCommandHandler(dbContext);

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

        var existingLabel = new WorkflowLabelDefinitionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "label-1",
            Type = LabelType.String
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Labels.Add(existingLabel);
        await dbContext.SaveChangesAsync();

        var command = new AddWorkflowLabelDefinitionCommand(
            WorkflowVersionId: version.Id,
            Key: "label-1",
            Type: LabelType.Int,
            Description: null
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

