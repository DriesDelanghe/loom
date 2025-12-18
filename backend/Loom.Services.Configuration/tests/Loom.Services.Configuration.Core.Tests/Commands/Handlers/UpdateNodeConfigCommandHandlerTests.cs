using System.Text.Json;
using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class UpdateNodeConfigCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_UpdatesNodeConfig_InDraftVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new UpdateNodeConfigCommandHandler(dbContext);

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
            ConfigJson = null,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Nodes.Add(node);
        await dbContext.SaveChangesAsync();

        var newConfig = JsonDocument.Parse("""{"key": "value"}""");
        var command = new UpdateNodeConfigCommand(NodeId: node.Id, Config: newConfig);

        var result = await handler.HandleAsync(command);

        Assert.True(result);
        
        var updatedNode = await dbContext.Nodes.FindAsync(node.Id);
        Assert.NotNull(updatedNode);
        Assert.NotNull(updatedNode!.ConfigJson);
        Assert.Contains("key", updatedNode.ConfigJson);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNodeNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new UpdateNodeConfigCommandHandler(dbContext);

        var command = new UpdateNodeConfigCommand(NodeId: Guid.NewGuid(), Config: null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNotDraft()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new UpdateNodeConfigCommandHandler(dbContext);

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

        var node = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "node-1",
            Type = NodeType.Action,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Nodes.Add(node);
        await dbContext.SaveChangesAsync();

        var command = new UpdateNodeConfigCommand(NodeId: node.Id, Config: null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

