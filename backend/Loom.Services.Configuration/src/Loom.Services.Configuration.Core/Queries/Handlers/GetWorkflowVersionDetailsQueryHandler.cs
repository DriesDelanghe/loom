using Microsoft.EntityFrameworkCore;

namespace Loom.Services.Configuration.Core.Queries.Handlers;

public class GetWorkflowVersionDetailsQueryHandler : IQueryHandler<GetWorkflowVersionDetailsQuery, WorkflowVersionDetailsDto>
{
    private readonly ConfigurationDbContext _dbContext;

    public GetWorkflowVersionDetailsQueryHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkflowVersionDetailsDto> HandleAsync(GetWorkflowVersionDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.WorkflowVersions
            .Include(v => v.Nodes)
            .Include(v => v.Connections)
            .Include(v => v.Variables)
            .Include(v => v.Labels)
            .Include(v => v.Settings)
            .Include(v => v.TriggerBindings)
                .ThenInclude(tb => tb.NodeBindings)
            .Include(v => v.TriggerBindings)
                .ThenInclude(tb => tb.Trigger)
            .FirstOrDefaultAsync(v => v.Id == query.WorkflowVersionId, cancellationToken);

        if (version == null)
            throw new InvalidOperationException($"Workflow version {query.WorkflowVersionId} not found");

        var triggerBindings = version.TriggerBindings.Select(tb => new TriggerBindingDto(
            tb.Id,
            tb.TriggerId,
            tb.WorkflowVersionId,
            tb.Enabled,
            tb.Priority,
            tb.NodeBindings.OrderBy(nb => nb.Order).Select(nb => nb.ToDomain()).ToList(),
            tb.Trigger.Type.ToString(),
            tb.Trigger.ConfigJson != null ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(tb.Trigger.ConfigJson) : null
        )).ToList();

        return new WorkflowVersionDetailsDto(
            version.ToDomain(),
            version.Nodes.Select(n => n.ToDomain()).ToList(),
            version.Connections.Select(c => c.ToDomain()).ToList(),
            version.Variables.Select(v => v.ToDomain()).ToList(),
            version.Labels.Select(l => l.ToDomain()).ToList(),
            version.Settings?.ToDomain(),
            triggerBindings
        );
    }
}
