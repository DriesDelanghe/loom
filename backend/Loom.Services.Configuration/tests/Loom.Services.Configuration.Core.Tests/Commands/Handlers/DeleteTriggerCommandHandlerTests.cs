using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Triggers;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class DeleteTriggerCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_DeletesTrigger_WithoutBindings()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new DeleteTriggerCommandHandler(dbContext);

        var trigger = new TriggerEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = TriggerType.Webhook,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Triggers.Add(trigger);
        await dbContext.SaveChangesAsync();

        var command = new DeleteTriggerCommand(TriggerId: trigger.Id);
        var result = await handler.HandleAsync(command);

        Assert.True(result);
        
        var deletedTrigger = await dbContext.Triggers.FindAsync(trigger.Id);
        Assert.Null(deletedTrigger);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenTriggerHasBindings()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new DeleteTriggerCommandHandler(dbContext);

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
            Status = Domain.Workflows.WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var trigger = new TriggerEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = TriggerType.Webhook,
            CreatedAt = DateTime.UtcNow
        };

        var binding = new TriggerBindingEntity
        {
            Id = Guid.NewGuid(),
            TriggerId = trigger.Id,
            WorkflowVersionId = version.Id,
            Enabled = true
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Triggers.Add(trigger);
        dbContext.TriggerBindings.Add(binding);
        await dbContext.SaveChangesAsync();

        var command = new DeleteTriggerCommand(TriggerId: trigger.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

