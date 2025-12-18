using Loom.Services.Configuration.Domain.Graph;

namespace Loom.Services.Configuration.Core.Commands;

public record UpdateNodeMetadataCommand(
    Guid NodeId,
    string? Name,
    NodeType? Type
);



