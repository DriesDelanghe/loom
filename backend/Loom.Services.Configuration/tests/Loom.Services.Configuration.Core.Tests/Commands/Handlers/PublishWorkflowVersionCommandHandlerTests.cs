using Loom.Services.Configuration.Core.Commands;
using Loom.Services.Configuration.Core.Commands.Handlers;
using Loom.Services.Configuration.Core.Services;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;
using Moq;

namespace Loom.Services.Configuration.Core.Tests.Commands.Handlers;

public class PublishWorkflowVersionCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_PublishesDraftVersion_WhenValid()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validatorMock = new Mock<IWorkflowValidator>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(true, new List<string>(), new List<string>()));

        var handler = new PublishWorkflowVersionCommandHandler(dbContext, validatorMock.Object);

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

        var command = new PublishWorkflowVersionCommand(
            WorkflowVersionId: version.Id,
            PublishedBy: "test-user"
        );

        var result = await handler.HandleAsync(command);

        Assert.True(result);
        
        var updatedVersion = await dbContext.WorkflowVersions.FindAsync(version.Id);
        Assert.NotNull(updatedVersion);
        Assert.Equal(WorkflowStatus.Published, updatedVersion!.Status);
        Assert.NotNull(updatedVersion.PublishedAt);
        Assert.Equal("test-user", updatedVersion.PublishedBy);
    }

    [Fact]
    public async Task HandleAsync_ArchivesPreviousPublishedVersion()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validatorMock = new Mock<IWorkflowValidator>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(true, new List<string>(), new List<string>()));

        var handler = new PublishWorkflowVersionCommandHandler(dbContext, validatorMock.Object);

        var definition = new WorkflowDefinitionEntity
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Name = "Test",
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var oldVersion = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Version = 1,
            Status = WorkflowStatus.Published,
            CreatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow.AddDays(-1)
        };

        var newVersion = new WorkflowVersionEntity
        {
            Id = Guid.NewGuid(),
            DefinitionId = definition.Id,
            Version = 2,
            Status = WorkflowStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkflowDefinitions.Add(definition);
        dbContext.WorkflowVersions.AddRange(oldVersion, newVersion);
        await dbContext.SaveChangesAsync();

        var command = new PublishWorkflowVersionCommand(
            WorkflowVersionId: newVersion.Id,
            PublishedBy: "test-user"
        );

        await handler.HandleAsync(command);

        var archivedVersion = await dbContext.WorkflowVersions.FindAsync(oldVersion.Id);
        Assert.NotNull(archivedVersion);
        Assert.Equal(WorkflowStatus.Archived, archivedVersion!.Status);
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenVersionNotFound()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validatorMock = new Mock<IWorkflowValidator>();
        var handler = new PublishWorkflowVersionCommandHandler(dbContext, validatorMock.Object);

        var command = new PublishWorkflowVersionCommand(
            WorkflowVersionId: Guid.NewGuid(),
            PublishedBy: "test-user"
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenNotDraft()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validatorMock = new Mock<IWorkflowValidator>();
        var handler = new PublishWorkflowVersionCommandHandler(dbContext, validatorMock.Object);

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

        var command = new PublishWorkflowVersionCommand(
            WorkflowVersionId: version.Id,
            PublishedBy: "test-user"
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_ThrowsException_WhenValidationFails()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validatorMock = new Mock<IWorkflowValidator>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(false, new List<string> { "Validation error" }, new List<string>()));

        var handler = new PublishWorkflowVersionCommandHandler(dbContext, validatorMock.Object);

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

        var command = new PublishWorkflowVersionCommand(
            WorkflowVersionId: version.Id,
            PublishedBy: "test-user"
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}

