using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Observability;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class RemoveWorkflowLabelDefinitionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_RemovesLabel_FromDraftVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new RemoveWorkflowLabelDefinitionCommandHandler(dbContext);

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

        var label = new WorkflowLabelDefinitionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "label-1",
            Type = LabelType.String
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Labels.Add(label);
        await dbContext.SaveChangesAsync();

        var command = new RemoveWorkflowLabelDefinitionCommand(LabelId: label.Id);
        var result = await handler.HandleAsync(command);

        Assert.True(result);
        
        var removedLabel = await dbContext.Labels.FindAsync(label.Id);
        Assert.Null(removedLabel);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNotDraft()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new RemoveWorkflowLabelDefinitionCommandHandler(dbContext);

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

        var label = new WorkflowLabelDefinitionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "label-1",
            Type = LabelType.String
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Labels.Add(label);
        await dbContext.SaveChangesAsync();

        var command = new RemoveWorkflowLabelDefinitionCommand(LabelId: label.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

