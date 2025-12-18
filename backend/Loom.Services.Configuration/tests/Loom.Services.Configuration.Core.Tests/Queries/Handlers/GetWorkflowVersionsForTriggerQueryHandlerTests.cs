using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Core.Queries.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Triggers;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Queries.Handlers;

public class GetWorkflowVersionsForTriggerQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPublishedVersions_ForTrigger()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetWorkflowVersionsForTriggerQueryHandler(dbContext);

        var tenantId = Guid.NewGuid();
        var definition = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Test",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var publishedVersion = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Version = 1,
            Status = WorkflowStatus.Published,
            CreatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow
        };

        var draftVersion = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Version = 2,
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var trigger = new TriggerEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Type = TriggerType.Webhook,
            CreatedAt = DateTime.UtcNow
        };

        var enabledBinding = new TriggerBindingEntity
        {
            Id = Guid.NewGuid(),
            TriggerId = trigger.Id,
            WorkflowVersionId = publishedVersion.Id,
            Enabled = true,
            Priority = 1
        };

        var disabledBinding = new TriggerBindingEntity
        {
            Id = Guid.NewGuid(),
            TriggerId = trigger.Id,
            WorkflowVersionId = draftVersion.Id,
            Enabled = false
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.AddRange(publishedVersion, draftVersion);
        dbContext.Triggers.Add(trigger);
        dbContext.TriggerBindings.AddRange(enabledBinding, disabledBinding);
        await dbContext.SaveChangesAsync();

        var query = new GetWorkflowVersionsForTriggerQuery(TriggerId: trigger.Id);
        var result = await handler.HandleAsync(query);

        Assert.Single(result);
        Assert.Equal(publishedVersion.Id, result[0].WorkflowVersionId);
        Assert.Equal(tenantId, result[0].TenantId);
        Assert.Equal(1, result[0].Priority);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenNoPublishedBindings()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetWorkflowVersionsForTriggerQueryHandler(dbContext);

        var trigger = new TriggerEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Type = TriggerType.Webhook,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Triggers.Add(trigger);
        await dbContext.SaveChangesAsync();

        var query = new GetWorkflowVersionsForTriggerQuery(TriggerId: trigger.Id);
        var result = await handler.HandleAsync(query);

        Assert.Empty(result);
    }
}

