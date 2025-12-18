using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Observability;
using Loom.Services.Configuration.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class CreateDraftWorkflowVersionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesDraftVersion_FromPublishedVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new CreateDraftWorkflowVersionCommandHandler(dbContext);

        var definition = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
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

        var node1 = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = publishedVersion.Id,
            Key = "node-1",
            Type = NodeType.Action,
            CreatedAt = DateTime.UtcNow
        };

        var node2 = new NodeEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = publishedVersion.Id,
            Key = "node-2",
            Type = NodeType.Condition,
            CreatedAt = DateTime.UtcNow
        };

        var connection = new ConnectionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = publishedVersion.Id,
            FromNodeId = node1.Id,
            ToNodeId = node2.Id,
            On = ConnectionType.Success
        };

        var variable = new WorkflowVariableEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = publishedVersion.Id,
            Key = "var-1",
            Type = VariableType.String
        };

        var label = new WorkflowLabelDefinitionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = publishedVersion.Id,
            Key = "label-1",
            Type = LabelType.String
        };

        var settings = new WorkflowSettingsEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = publishedVersion.Id,
            MaxNodeExecutions = 100
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.Add(publishedVersion);
        dbContext.Nodes.AddRange(node1, node2);
        dbContext.Connections.Add(connection);
        dbContext.Variables.Add(variable);
        dbContext.Labels.Add(label);
        dbContext.Settings.Add(settings);
        await dbContext.SaveChangesAsync();

        var command = new CreateDraftWorkflowVersionCommand(
            WorkflowDefinitionId: definition.Id,
            CreatedBy: "test-user"
        );

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result);
        
        var newVersion = await dbContext.WorkflowVersions
            .Include(v => v.Nodes)
            .Include(v => v.Connections)
            .Include(v => v.Variables)
            .Include(v => v.Labels)
            .Include(v => v.Settings)
            .FirstOrDefaultAsync(v => v.Id == result);

        Assert.NotNull(newVersion);
        Assert.Equal(2, newVersion!.Version);
        Assert.Equal(WorkflowStatus.Draft, newVersion.Status);
        Assert.Equal("test-user", newVersion.CreatedBy);
        Assert.Equal(2, newVersion.Nodes.Count);
        Assert.Single(newVersion.Connections);
        Assert.Single(newVersion.Variables);
        Assert.Single(newVersion.Labels);
        Assert.NotNull(newVersion.Settings);
        Assert.Equal(100, newVersion.Settings!.MaxNodeExecutions);
    }

    [Fact]
    public async Task HandleAsync_CreatesVersion1_WhenNoPublishedVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new CreateDraftWorkflowVersionCommandHandler(dbContext);

        var definition = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(definition);
        await dbContext.SaveChangesAsync();

        var command = new CreateDraftWorkflowVersionCommand(
            WorkflowDefinitionId: definition.Id,
            CreatedBy: "test-user"
        );

        var result = await handler.HandleAsync(command);

        var newVersion = await dbContext.WorkflowVersions.FindAsync(result);
        Assert.NotNull(newVersion);
        Assert.Equal(1, newVersion!.Version);
        Assert.Equal(WorkflowStatus.Draft, newVersion.Status);
    }
}

