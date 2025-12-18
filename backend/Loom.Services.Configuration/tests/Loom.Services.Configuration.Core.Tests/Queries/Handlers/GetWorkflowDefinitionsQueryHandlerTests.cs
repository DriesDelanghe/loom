using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Core.Queries.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Tests.Queries.Handlers;

public class GetWorkflowDefinitionsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsWorkflowDefinitions_ForTenant()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetWorkflowDefinitionsQueryHandler(dbContext);

        var tenantId = Guid.NewGuid();
        var definition1 = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Workflow 1",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var definition2 = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Workflow 2",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var version1 = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition1.Id,
            Version = 1,
            Status = WorkflowStatus.Published,
            CreatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow
        };

        var version2 = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition2.Id,
            Version = 1,
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.AddRange(definition1, definition2);
        dbContext.WorkflowVersions.AddRange(version1, version2);
        await dbContext.SaveChangesAsync();

        var query = new GetWorkflowDefinitionsQuery(TenantId: tenantId);
        var result = await handler.HandleAsync(query);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, d => d.Id == definition1.Id && d.HasPublishedVersion);
        Assert.Contains(result, d => d.Id == definition2.Id && !d.HasPublishedVersion);
    }

    [Fact]
    public async Task HandleAsync_ExcludesArchivedDefinitions()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new GetWorkflowDefinitionsQueryHandler(dbContext);

        var tenantId = Guid.NewGuid();
        var activeDefinition = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Active",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var archivedDefinition = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Archived",
            IsArchived = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.AddRange(activeDefinition, archivedDefinition);
        await dbContext.SaveChangesAsync();

        var query = new GetWorkflowDefinitionsQuery(TenantId: tenantId);
        var result = await handler.HandleAsync(query);

        Assert.Single(result);
        Assert.Equal(activeDefinition.Id, result[0].Id);
    }
}

