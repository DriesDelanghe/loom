using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class RemoveNodeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_RemovesNode_AndRelatedConnections()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new RemoveNodeCommandHandler(dbContext);

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

        var node1 = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "node-1",
            Type = NodeType.Action,
            CreatedAt = DateTime.UtcNow
        };

        var node2 = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "node-2",
            Type = NodeType.Action,
            CreatedAt = DateTime.UtcNow
        };

        var connection1 = new ConnectionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            FromNodeId = node1.Id,
            ToNodeId = node2.Id,
            On = ConnectionType.Success
        };

        var connection2 = new ConnectionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            FromNodeId = node2.Id,
            ToNodeId = node1.Id,
            On = ConnectionType.Success
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Nodes.AddRange(node1, node2);
        dbContext.Connections.AddRange(connection1, connection2);
        await dbContext.SaveChangesAsync();

        var command = new RemoveNodeCommand(NodeId: node1.Id);
        var result = await handler.HandleAsync(command);

        Assert.True(result);
        
        var removedNode = await dbContext.Nodes.FindAsync(node1.Id);
        Assert.Null(removedNode);

        var remainingConnections = await dbContext.Connections
            .Where(c => c.WorkflowVersionId == version.Id)
            .ToListAsync();
        
        Assert.Empty(remainingConnections);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNodeNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new RemoveNodeCommandHandler(dbContext);

        var command = new RemoveNodeCommand(NodeId: Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNotDraft()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new RemoveNodeCommandHandler(dbContext);

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

        var command = new RemoveNodeCommand(NodeId: node.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

