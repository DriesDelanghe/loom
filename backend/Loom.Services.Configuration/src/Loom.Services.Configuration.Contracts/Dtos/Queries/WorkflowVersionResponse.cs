namespace Loom.Services.Configuration.Contracts.Dtos.Queries;

public record WorkflowVersionResponse(
    Guid Id,
    int Version,
    string Status,
    DateTime CreatedAt,
    DateTime? PublishedAt
);

