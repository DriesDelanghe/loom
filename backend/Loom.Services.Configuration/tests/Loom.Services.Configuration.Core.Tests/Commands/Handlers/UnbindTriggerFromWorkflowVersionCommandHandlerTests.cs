using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Triggers;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class UnbindTriggerFromWorkflowVersionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_UnbindsTrigger_FromWorkflowVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new UnbindTriggerFromWorkflowVersionCommandHandler(dbContext);

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

        var command = new UnbindTriggerFromWorkflowVersionCommand(TriggerBindingId: binding.Id);
        var result = await handler.HandleAsync(command);

        Assert.True(result);
        
        var removedBinding = await dbContext.TriggerBindings.FindAsync(binding.Id);
        Assert.Null(removedBinding);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenBindingNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new UnbindTriggerFromWorkflowVersionCommandHandler(dbContext);

        var command = new UnbindTriggerFromWorkflowVersionCommand(TriggerBindingId: Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

