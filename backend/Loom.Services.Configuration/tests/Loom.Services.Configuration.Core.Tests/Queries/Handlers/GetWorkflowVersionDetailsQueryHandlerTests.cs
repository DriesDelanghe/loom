using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Core.Queries.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Observability;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Triggers;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Queries.Handlers;

public class GetWorkflowVersionDetailsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsCompleteWorkflowVersionDetails()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetWorkflowVersionDetailsQueryHandler(dbContext);

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

        var node = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "node-1",
            Type = NodeType.Action,
            CreatedAt = DateTime.UtcNow
        };

        var connection = new ConnectionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            FromNodeId = node.Id,
            ToNodeId = node.Id,
            On = ConnectionType.Success
        };

        var variable = new WorkflowVariableEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "var-1",
            Type = VariableType.String
        };

        var label = new WorkflowLabelDefinitionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "label-1",
            Type = LabelType.String
        };

        var settings = new WorkflowSettingsEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            MaxNodeExecutions = 100
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
        dbContext.Nodes.Add(node);
        dbContext.Connections.Add(connection);
        dbContext.Variables.Add(variable);
        dbContext.Labels.Add(label);
        dbContext.Settings.Add(settings);
        dbContext.Triggers.Add(trigger);
        dbContext.TriggerBindings.Add(binding);
        await dbContext.SaveChangesAsync();

        var query = new GetWorkflowVersionDetailsQuery(WorkflowVersionId: version.Id);
        var result = await handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal(version.Id, result.Version.Id);
        Assert.Single(result.Nodes);
        Assert.Single(result.Connections);
        Assert.Single(result.Variables);
        Assert.Single(result.Labels);
        Assert.NotNull(result.Settings);
        Assert.Single(result.TriggerBindings);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenVersionNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetWorkflowVersionDetailsQueryHandler(dbContext);

        var query = new GetWorkflowVersionDetailsQuery(WorkflowVersionId: Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(query));
    }
}

