using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Domain.Persistence;

public class WorkflowSettingsEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public int? MaxNodeExecutions { get; set; }
    public int? MaxDurationSeconds { get; set; }

    public WorkflowVersionEntity WorkflowVersion { get; set; } = null!;

    public WorkflowSettings ToDomain()
    {
        return new WorkflowSettings
        {
            WorkflowVersionId = WorkflowVersionId,
            MaxNodeExecutions = MaxNodeExecutions,
            MaxDurationSeconds = MaxDurationSeconds
        };
    }

    public static WorkflowSettingsEntity FromDomain(WorkflowSettings settings, Guid versionId)
    {
        return new WorkflowSettingsEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = versionId,
            MaxNodeExecutions = settings.MaxNodeExecutions,
            MaxDurationSeconds = settings.MaxDurationSeconds
        };
    }
}


