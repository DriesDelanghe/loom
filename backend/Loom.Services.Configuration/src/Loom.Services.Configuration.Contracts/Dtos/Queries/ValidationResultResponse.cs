namespace Loom.Services.Configuration.Contracts.Dtos.Queries;

public record ValidationResultResponse(
    bool IsValid,
    List<string> Errors,
    List<string> Warnings
);

