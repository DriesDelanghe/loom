using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Triggers;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class BindTriggerToWorkflowVersionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_BindsTrigger_ToWorkflowVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new BindTriggerToWorkflowVersionCommandHandler(dbContext);

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

        var trigger = new TriggerEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = TriggerType.Webhook,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Triggers.Add(trigger);
        await dbContext.SaveChangesAsync();

        var command = new BindTriggerToWorkflowVersionCommand(
            TriggerId: trigger.Id,
            WorkflowVersionId: version.Id,
            Priority: 1,
            Enabled: true
        );

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);
        
        var binding = await dbContext.TriggerBindings.FindAsync(result);
        Assert.NotNull(binding);
        Assert.Equal(trigger.Id, binding!.TriggerId);
        Assert.Equal(version.Id, binding.WorkflowVersionId);
        Assert.Equal(1, binding.Priority);
        Assert.True(binding.Enabled);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenBindingAlreadyExists()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new BindTriggerToWorkflowVersionCommandHandler(dbContext);

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

        var trigger = new TriggerEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = TriggerType.Webhook,
            CreatedAt = DateTime.UtcNow
        };

        var existingBinding = new TriggerBindingEntity
        {
            Id = Guid.NewGuid(),
            TriggerId = trigger.Id,
            WorkflowVersionId = version.Id,
            Enabled = true
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Triggers.Add(trigger);
        dbContext.TriggerBindings.Add(existingBinding);
        await dbContext.SaveChangesAsync();

        var command = new BindTriggerToWorkflowVersionCommand(
            TriggerId: trigger.Id,
            WorkflowVersionId: version.Id,
            Priority: null,
            Enabled: false
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

