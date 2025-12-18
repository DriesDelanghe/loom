using Loom.Services.Configuration.Domain.Graph;
using Loom.Services.Configuration.Domain.Observability;
using Loom.Services.Configuration.Domain.Triggers;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Queries;

public record GetWorkflowVersionDetailsQuery(
    Guid WorkflowVersionId
);

public record WorkflowVersionDetailsDto(
    WorkflowVersion Version,
    List<Node> Nodes,
    List<Connection> Connections,
    List<WorkflowVariable> Variables,
    List<WorkflowLabelDefinition> Labels,
    WorkflowSettings? Settings,
    List<TriggerBindingDto> TriggerBindings
);

public record TriggerBindingDto(
    Guid Id,
    Guid TriggerId,
    Guid WorkflowVersionId,
    bool Enabled,
    int? Priority,
    List<TriggerNodeBinding> NodeBindings,
    string TriggerType,
    Dictionary<string, object>? TriggerConfig
);


