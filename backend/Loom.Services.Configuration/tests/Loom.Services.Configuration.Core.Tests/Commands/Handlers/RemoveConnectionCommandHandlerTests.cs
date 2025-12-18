using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class RemoveConnectionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_RemovesConnection_FromDraftVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new RemoveConnectionCommandHandler(dbContext);

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

        var connection = new ConnectionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            FromNodeId = node1.Id,
            ToNodeId = node2.Id,
            On = ConnectionType.Success
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Nodes.AddRange(node1, node2);
        dbContext.Connections.Add(connection);
        await dbContext.SaveChangesAsync();

        var command = new RemoveConnectionCommand(ConnectionId: connection.Id);
        var result = await handler.HandleAsync(command);

        Assert.True(result);
        
        var removedConnection = await dbContext.Connections.FindAsync(connection.Id);
        Assert.Null(removedConnection);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenConnectionNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new RemoveConnectionCommandHandler(dbContext);

        var command = new RemoveConnectionCommand(ConnectionId: Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNotDraft()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new RemoveConnectionCommandHandler(dbContext);

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

        var connection = new ConnectionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            FromNodeId = node1.Id,
            ToNodeId = node2.Id,
            On = ConnectionType.Success
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Nodes.AddRange(node1, node2);
        dbContext.Connections.Add(connection);
        await dbContext.SaveChangesAsync();

        var command = new RemoveConnectionCommand(ConnectionId: connection.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

