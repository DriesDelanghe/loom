using System.Text.Json;
using Loom.Services.Configuration.Domain.Observability;

namespace Loom.Services.Configuration.Domain.Persistence;

public class WorkflowVariableEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public string Key { get; set; } = default!;
    public VariableType Type { get; set; }
    public string? InitialValueJson { get; set; }
    public string? Description { get; set; }

    public WorkflowVersionEntity WorkflowVersion { get; set; } = null!;

    public WorkflowVariable ToDomain()
    {
        return new WorkflowVariable
        {
            Id = Id,
            WorkflowVersionId = WorkflowVersionId,
            Key = Key,
            Type = Type,
            InitialValue = InitialValueJson != null ? JsonDocument.Parse(InitialValueJson) : null,
            Description = Description
        };
    }

    public static WorkflowVariableEntity FromDomain(WorkflowVariable variable)
    {
        return new WorkflowVariableEntity
        {
            Id = variable.Id,
            WorkflowVersionId = variable.WorkflowVersionId,
            Key = variable.Key,
            Type = variable.Type,
            InitialValueJson = variable.InitialValue?.RootElement.GetRawText(),
            Description = variable.Description
        };
    }
}


