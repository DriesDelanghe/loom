using Loom.Services.Configuration.Core.Queries;
using Loom.Services.Configuration.Core.Queries.Handlers;

namespace Loom.Services.Configuration.Core.Services;

public class WorkflowCompiler : IWorkflowCompiler
{
    private readonly GetCompiledWorkflowVersionQueryHandler _queryHandler;

    public WorkflowCompiler(GetCompiledWorkflowVersionQueryHandler queryHandler)
    {
        _queryHandler = queryHandler;
    }

    public async Task<CompiledWorkflowDto> CompileAsync(Guid workflowVersionId, CancellationToken cancellationToken = default)
    {
        var query = new GetCompiledWorkflowVersionQuery(workflowVersionId);
        return await _queryHandler.HandleAsync(query, cancellationToken);
    }
}


