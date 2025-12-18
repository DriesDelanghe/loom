using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class ArchiveWorkflowDefinitionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ArchivesWorkflowDefinition()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new ArchiveWorkflowDefinitionCommandHandler(dbContext);

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

        var command = new ArchiveWorkflowDefinitionCommand(WorkflowDefinitionId: definition.Id);
        var result = await handler.HandleAsync(command);

        Assert.True(result);
        
        var archived = await dbContext.WorkflowDefinitions.FindAsync(definition.Id);
        Assert.NotNull(archived);
        Assert.True(archived!.IsArchived);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenDefinitionNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var handler = new ArchiveWorkflowDefinitionCommandHandler(dbContext);

        var command = new ArchiveWorkflowDefinitionCommand(WorkflowDefinitionId: Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

