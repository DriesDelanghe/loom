using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Observability;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Contracts.Dtos.Queries;

public record CompiledWorkflowResponse(
    WorkflowVersion Version,
    List<Node> Nodes,
    List<Connection> Connections,
    List<WorkflowVariable> Variables,
    List<WorkflowLabelDefinition> Labels,
    WorkflowSettings? Settings,
    List<CompiledTriggerResponse> Triggers
);

public record CompiledTriggerResponse(
    Guid TriggerId,
    string Type,
    Dictionary<string, object>? Config,
    List<Guid> EntryNodeIds
);
