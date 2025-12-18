using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Observability;
using Loom.Services.Configuration.Domain.Triggers;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Contracts.Dtos.Queries;

public record WorkflowVersionDetailsResponse(
    WorkflowVersion Version,
    List<Node> Nodes,
    List<Connection> Connections,
    List<WorkflowVariable> Variables,
    List<WorkflowLabelDefinition> Labels,
    WorkflowSettings? Settings,
    List<TriggerBindingResponse> TriggerBindings
);

public record TriggerBindingResponse(
    Guid Id,
    Guid TriggerId,
    Guid WorkflowVersionId,
    bool Enabled,
    int? Priority,
    List<TriggerNodeBindingResponse> NodeBindings,
    string TriggerType,
    Dictionary<string, object>? TriggerConfig
);

public record TriggerNodeBindingResponse(
    Guid Id,
    Guid EntryNodeId,
    int Order
);
