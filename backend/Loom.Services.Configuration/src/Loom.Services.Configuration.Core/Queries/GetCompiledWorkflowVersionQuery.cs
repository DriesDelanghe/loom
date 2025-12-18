using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Observability;
using Loom.Services.Configuration.Domain.Triggers;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Queries;

public record GetCompiledWorkflowVersionQuery(
    Guid WorkflowVersionId
);

public record CompiledWorkflowDto(
    WorkflowVersion Version,
    List<Node> Nodes,
    List<Connection> Connections,
    List<WorkflowVariable> Variables,
    List<WorkflowLabelDefinition> Labels,
    WorkflowSettings? Settings,
    List<CompiledTriggerDto> Triggers
);

public record CompiledTriggerDto(
    Guid TriggerId,
    string Type,
    Dictionary<string, object>? Config,
    List<Guid> EntryNodeIds
);


