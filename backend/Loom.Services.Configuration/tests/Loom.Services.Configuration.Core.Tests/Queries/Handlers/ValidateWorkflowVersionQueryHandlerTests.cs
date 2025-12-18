using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Core.Queries.Handlers;
using Loom.Services.Configuration.Core.Services;
using Loom.Services.Configuration.Core.Tests.TestHelpers;
using Loom.Services.Configuration.Domain.Persistence;
using Loom.Services.Configuration.Domain.Workflows;
using Moq;

namespace Loom.Services.Configuration.Core.Tests.Queries.Handlers;

public class ValidateWorkflowVersionQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsValidationResult_FromValidator()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validatorMock = new Mock<IWorkflowValidator>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                IsValid: true,
                Errors: new List<string>(),
                Warnings: new List<string> { "Warning 1" }
            ));

        var handler = new ValidateWorkflowVersionQueryHandler(validatorMock.Object);

        var query = new ValidateWorkflowVersionQuery(WorkflowVersionId: Guid.NewGuid());
        var result = await handler.HandleAsync(query);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Single(result.Warnings);
        Assert.Equal("Warning 1", result.Warnings[0]);

        validatorMock.Verify(v => v.ValidateAsync(query.WorkflowVersionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WhenValidationFails()
    {
        using var dbContext = InMemoryDbContextFactory.Create();
        var validatorMock = new Mock<IWorkflowValidator>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                IsValid: false,
                Errors: new List<string> { "Error 1", "Error 2" },
                Warnings: new List<string>()
            ));

        var handler = new ValidateWorkflowVersionQueryHandler(validatorMock.Object);

        var query = new ValidateWorkflowVersionQuery(WorkflowVersionId: Guid.NewGuid());
        var result = await handler.HandleAsync(query);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Empty(result.Warnings);
    }
}

