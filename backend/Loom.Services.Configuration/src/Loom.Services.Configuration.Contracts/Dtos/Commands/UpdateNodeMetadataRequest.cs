using Loom.Services.Configuration.Domain.Graph;

namespace Loom.Services.Configuration.Contracts.Dtos.Commands;

public record UpdateNodeMetadataRequest(
    Guid NodeId,
    string? Name,
    NodeType? Type
);


