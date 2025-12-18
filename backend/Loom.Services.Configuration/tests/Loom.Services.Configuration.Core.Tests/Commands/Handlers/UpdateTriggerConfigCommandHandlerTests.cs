using System.Text.Json;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Triggers;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class UpdateTriggerConfigCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_UpdatesTriggerConfig()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new UpdateTriggerConfigCommandHandler(dbContext);

        var trigger = new TriggerEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = TriggerType.Webhook,
            ConfigJson = """{"old": "value"}""",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Triggers.Add(trigger);
        await dbContext.SaveChangesAsync();

        var newConfig = JsonDocument.Parse("""{"new": "value"}""");
        var command = new UpdateTriggerConfigCommand(
            TriggerId: trigger.Id,
            Config: newConfig
        );

        var result = await handler.HandleAsync(command);

        Assert.True(result);
        
        var updatedTrigger = await dbContext.Triggers.FindAsync(trigger.Id);
        Assert.NotNull(updatedTrigger);
        Assert.Contains("new", updatedTrigger!.ConfigJson!);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenTriggerNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new UpdateTriggerConfigCommandHandler(dbContext);

        var command = new UpdateTriggerConfigCommand(
            TriggerId: Guid.NewGuid(),
            Config: null
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

