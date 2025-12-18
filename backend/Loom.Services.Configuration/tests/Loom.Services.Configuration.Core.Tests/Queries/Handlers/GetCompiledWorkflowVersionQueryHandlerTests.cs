using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Core.Queries.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Queries.Handlers;

public class GetCompiledWorkflowVersionQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsCompiledWorkflow_ForPublishedVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetCompiledWorkflowVersionQueryHandler(dbContext);

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
            CreatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow
        };

        var node = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "node-1",
            Type = NodeType.Action,
            CreatedAt = DateTime.UtcNow
        };

        var variable = new WorkflowVariableEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "var-1",
            Type = Domain.Observability.VariableType.String
        };

        var settings = new WorkflowSettingsEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            MaxNodeExecutions = 100
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Nodes.Add(node);
        dbContext.Variables.Add(variable);
        dbContext.Settings.Add(settings);
        await dbContext.SaveChangesAsync();

        var query = new GetCompiledWorkflowVersionQuery(WorkflowVersionId: version.Id);
        var result = await handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal(version.Id, result.Version.Id);
        Assert.Single(result.Nodes);
        Assert.Equal("node-1", result.Nodes[0].Key);
        Assert.Single(result.Variables);
        Assert.NotNull(result.Settings);
        Assert.Equal(100, result.Settings.MaxNodeExecutions);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenVersionNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetCompiledWorkflowVersionQueryHandler(dbContext);

        var query = new GetCompiledWorkflowVersionQuery(WorkflowVersionId: Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(query));
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNotPublished()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetCompiledWorkflowVersionQueryHandler(dbContext);

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

        var query = new GetCompiledWorkflowVersionQuery(WorkflowVersionId: version.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(query));
    }
}

