using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Core.Queries.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Queries.Handlers;

public class GetWorkflowVersionsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsVersions_ForWorkflowDefinition()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetWorkflowVersionsQueryHandler(dbContext);

        var definition = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var version1 = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Version = 1,
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var version2 = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Version = 2,
            Status = WorkflowStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            PublishedAt = DateTime.UtcNow.AddDays(-1)
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.AddRange(version1, version2);
        await dbContext.SaveChangesAsync();

        var query = new GetWorkflowVersionsQuery(WorkflowDefinitionId: definition.Id);
        var result = await handler.HandleAsync(query);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].Version);
        Assert.Equal(1, result[1].Version);
        Assert.Equal("Published", result[0].Status);
        Assert.Equal("Draft", result[1].Status);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenNoVersions()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetWorkflowVersionsQueryHandler(dbContext);

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

        var query = new GetWorkflowVersionsQuery(WorkflowDefinitionId: definition.Id);
        var result = await handler.HandleAsync(query);

        Assert.Empty(result);
    }
}

