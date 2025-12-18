using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Loom.Services.Configuration.Domain.Workflows;

namespace Loom.Services.Configuration.Core.Queries.Handlers;

public class GetCompiledWorkflowVersionQueryHandler : IQueryHandler<GetCompiledWorkflowVersionQuery, CompiledWorkflowDto>
{
    private readonly ConfigurationDbContext _dbContext;

    public GetCompiledWorkflowVersionQueryHandler(ConfigurationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CompiledWorkflowDto> HandleAsync(GetCompiledWorkflowVersionQuery query, CancellationToken cancellationToken = default)
    {
        var version = await _dbContext.WorkflowVersions
            .Include(v => v.Nodes)
            .Include(v => v.Connections)
            .Include(v => v.Variables)
            .Include(v => v.Labels)
            .Include(v => v.Settings)
            .Include(v => v.TriggerBindings)
                .ThenInclude(tb => tb.Trigger)
            .Include(v => v.TriggerBindings)
                .ThenInclude(tb => tb.NodeBindings)
            .FirstOrDefaultAsync(v => v.Id == query.WorkflowVersionId && v.Status == WorkflowStatus.Published, cancellationToken);

        if (version == null)
            throw new InvalidOperationException($"Published workflow version {query.WorkflowVersionId} not found");

        var triggers = version.TriggerBindings
            .Where(tb => tb.Enabled)
            .Select(tb =>
            {
                var trigger = tb.Trigger.ToDomain();
                Dictionary<string, object>? configDict = null;
                if (trigger.Config != null)
                {
                    configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(trigger.Config.RootElement.GetRawText());
                }

                return new CompiledTriggerDto(
                    tb.TriggerId,
                    tb.Trigger.Type.ToString(),
                    configDict,
                    tb.NodeBindings.OrderBy(nb => nb.Order).Select(nb => nb.EntryNodeId).ToList()
                );
            }).ToList();

        return new CompiledWorkflowDto(
            version.ToDomain(),
            version.Nodes.Select(n => n.ToDomain()).ToList(),
            version.Connections.Select(c => c.ToDomain()).ToList(),
            version.Variables.Select(v => v.ToDomain()).ToList(),
            version.Labels.Select(l => l.ToDomain()).ToList(),
            version.Settings?.ToDomain(),
            triggers
        );
    }
}
