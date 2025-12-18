using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class AddConnectionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_AddsConnection_BetweenNodes()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new AddConnectionCommandHandler(dbContext);

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

        var fromNode = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "node-1",
            Type = NodeType.Action,
            CreatedAt = DateTime.UtcNow
        };

        var toNode = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "node-2",
            Type = NodeType.Action,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Nodes.AddRange(fromNode, toNode);
        await dbContext.SaveChangesAsync();

        var command = new AddConnectionCommand(
            WorkflowVersionId: version.Id,
            FromNodeId: fromNode.Id,
            ToNodeId: toNode.Id,
            ConnectionType: ConnectionType.Success,
            Order: 1
        );

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);
        
        var connection = await dbContext.Connections.FindAsync(result);
        Assert.NotNull(connection);
        Assert.Equal(fromNode.Id, connection!.FromNodeId);
        Assert.Equal(toNode.Id, connection.ToNodeId);
        Assert.Equal(ConnectionType.Success, connection.On);
        Assert.Equal(1, connection.Order);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNodeNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new AddConnectionCommandHandler(dbContext);

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

        var command = new AddConnectionCommand(
            WorkflowVersionId: version.Id,
            FromNodeId: Guid.NewGuid(),
            ToNodeId: Guid.NewGuid(),
            ConnectionType: ConnectionType.Success,
            Order: null
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

