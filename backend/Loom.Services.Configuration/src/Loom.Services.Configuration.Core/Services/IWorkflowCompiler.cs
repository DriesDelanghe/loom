using Loom.Services.Configuration.Core.Queries;

namespace Loom.Services.Configuration.Core.Services;

public interface IWorkflowCompiler
{
    Task<CompiledWorkflowDto> CompileAsync(Guid workflowVersionId, CancellationToken cancellationToken = default);
}


