using System.Text.Json;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Triggers;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class CreateTriggerCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesTrigger()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new CreateTriggerCommandHandler(dbContext);

        var config = JsonDocument.Parse("""{"path": "/webhook"}""");
        var command = new CreateTriggerCommand(
            TenantId: Guid.NewGuid(),
            Type: TriggerType.Webhook,
            Config: config
        );

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);
        
        var trigger = await dbContext.Triggers.FindAsync(result);
        Assert.NotNull(trigger);
        Assert.Equal(command.TenantId, trigger!.TenantId);
        Assert.Equal(TriggerType.Webhook, trigger.Type);
        Assert.NotNull(trigger.ConfigJson);
    }

    [Fact]
    public async Task HandleAsync_CreatesTrigger_WithoutConfig()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new CreateTriggerCommandHandler(dbContext);

        var command = new CreateTriggerCommand(
            TenantId: Guid.NewGuid(),
            Type: TriggerType.Manual,
            Config: null
        );

        var result = await handler.HandleAsync(command);

        var trigger = await dbContext.Triggers.FindAsync(result);
        Assert.NotNull(trigger);
        Assert.Null(trigger!.ConfigJson);
    }
}

