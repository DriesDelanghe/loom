using Loom.Services.Configuration.Core.Services;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Services;

public class WorkflowValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ReturnsValid_ForValidWorkflow()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validator = new WorkflowValidator(dbContext);

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

        var result = await validator.ValidateAsync(version.Id);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenConnectionReferencesNonExistentNode()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validator = new WorkflowValidator(dbContext);

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

        var connection = new ConnectionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            FromNodeId = node1.Id,
            ToNodeId = Guid.NewGuid(),
            On = ConnectionType.Success
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Nodes.Add(node1);
        dbContext.Connections.Add(connection);
        await dbContext.SaveChangesAsync();

        var result = await validator.ValidateAsync(version.Id);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("non-existent"));
    }

    [Fact]
    public async Task ValidateAsync_ReturnsError_WhenDuplicateNodeKeys()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validator = new WorkflowValidator(dbContext);

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
            Key = "node-1",
            Type = NodeType.Action,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(version);
        dbContext.Nodes.AddRange(node1, node2);
        await dbContext.SaveChangesAsync();

        var result = await validator.ValidateAsync(version.Id);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Duplicate node keys"));
    }

    [Fact]
    public async Task ValidateAsync_ReturnsWarning_WhenUnreachableNodes()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validator = new WorkflowValidator(dbContext);

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

        var node3 = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = version.Id,
            Key = "node-3",
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
        dbContext.Nodes.AddRange(node1, node2, node3);
        dbContext.Connections.Add(connection);
        await dbContext.SaveChangesAsync();

        var result = await validator.ValidateAsync(version.Id);

        Assert.Contains(result.Warnings, w => w.Contains("isolated") || w.Contains("unreachable"));
    }

    [Fact]
    public async Task ValidateAsync_ReturnsWarning_WhenCyclesDetected()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validator = new WorkflowValidator(dbContext);

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

        var result = await validator.ValidateAsync(version.Id);

        Assert.Contains(result.Warnings, w => w.Contains("cycle"));
    }
}

