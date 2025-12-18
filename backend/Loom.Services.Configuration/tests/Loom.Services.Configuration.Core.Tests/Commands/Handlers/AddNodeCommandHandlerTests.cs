using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class AddNodeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsNode_ToDraftVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new AddNodeCommandHandler(dbContext);

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

        var command = new AddNodeCommand(
            WorkflowVersionId: version.Id,
            Key: "node-1",
            Name: "Test Node",
            Type: NodeType.Action,
            Config: null
        );

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);
        
        var node = await dbContext.Nodes.FindAsync(result);
        Assert.NotNull(node);
        Assert.Equal(version.Id, node!.WorkflowVersionId);
        Assert.Equal("node-1", node.Key);
        Assert.Equal("Test Node", node.Name);
        Assert.Equal(NodeType.Action, node.Type);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenVersionNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new AddNodeCommandHandler(dbContext);

        var command = new AddNodeCommand(
            WorkflowVersionId: Guid.NewGuid(),
            Key: "node-1",
            Name: null,
            Type: NodeType.Action,
            Config: null
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNotDraft()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new AddNodeCommandHandler(dbContext);

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

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        await dbContext.SaveChangesAsync();

        var command = new AddNodeCommand(
            WorkflowVersionId: version.Id,
            Key: "node-1",
            Name: null,
            Type: NodeType.Action,
            Config: null
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenKeyExists()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new AddNodeCommandHandler(dbContext);

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

        var existingNode = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "node-1",
            Type = NodeType.Action,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Nodes.Add(existingNode);
        await dbContext.SaveChangesAsync();

        var command = new AddNodeCommand(
            WorkflowVersionId: version.Id,
            Key: "node-1",
            Name: null,
            Type: NodeType.Condition,
            Config: null
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

