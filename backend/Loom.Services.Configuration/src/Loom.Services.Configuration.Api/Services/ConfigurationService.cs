using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Loom.Services.Configuration.Api.Services;

[Service]
public interface IConfigurationService
{
    [Operation]
    Task<IdResponse> CreateWorkflowDefinition(CreateWorkflowDefinitionRequest request, CallContext context = default);

    [Operation]
    Task<IdResponse> CreateDraftWorkflowVersion(CreateDraftWorkflowVersionRequest request, CallContext context = default);

    [Operation]
    Task<SuccessResponse> PublishWorkflowVersion(PublishWorkflowVersionRequest request, CallContext context = default);

    [Operation]
    Task<SuccessResponse> DeleteWorkflowVersion(DeleteWorkflowVersionRequest request, CallContext context = default);

    [Operation]
    Task<GetWorkflowDefinitionsResponse> GetWorkflowDefinitions(GetWorkflowDefinitionsRequest request, CallContext context = default);

    [Operation]
    Task<GetWorkflowVersionsResponse> GetWorkflowVersions(GetWorkflowVersionsRequest request, CallContext context = default);

    [Operation]
    Task<GetCompiledWorkflowResponse> GetCompiledWorkflow(GetCompiledWorkflowRequest request, CallContext context = default);

    [Operation]
    Task<ValidationResultResponse> ValidateWorkflowVersion(ValidateWorkflowVersionRequest request, CallContext context = default);
}

[DataContract]
public class IdResponse
{
    [DataMember(Order = 1)]
    public string Id { get; set; } = string.Empty;
}

[DataContract]
public class SuccessResponse
{
    [DataMember(Order = 1)]
    public bool Success { get; set; }
}

[DataContract]
public class CreateWorkflowDefinitionRequest
{
    [DataMember(Order = 1)]
    public string TenantId { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string? Description { get; set; }
}

[DataContract]
public class CreateDraftWorkflowVersionRequest
{
    [DataMember(Order = 1)]
    public string WorkflowDefinitionId { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string CreatedBy { get; set; } = string.Empty;
}

[DataContract]
public class PublishWorkflowVersionRequest
{
    [DataMember(Order = 1)]
    public string WorkflowVersionId { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string PublishedBy { get; set; } = string.Empty;
}

[DataContract]
public class DeleteWorkflowVersionRequest
{
    [DataMember(Order = 1)]
    public string WorkflowVersionId { get; set; } = string.Empty;
}

[DataContract]
public class GetWorkflowDefinitionsRequest
{
    [DataMember(Order = 1)]
    public string TenantId { get; set; } = string.Empty;
}

[DataContract]
public class GetWorkflowDefinitionsResponse
{
    [DataMember(Order = 1)]
    public List<WorkflowDefinition> Definitions { get; set; } = new();
}

[DataContract]
public class WorkflowDefinition
{
    [DataMember(Order = 1)]
    public string Id { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public bool HasPublishedVersion { get; set; }

    [DataMember(Order = 4)]
    public int LatestVersion { get; set; }
}

[DataContract]
public class GetWorkflowVersionsRequest
{
    [DataMember(Order = 1)]
    public string WorkflowDefinitionId { get; set; } = string.Empty;
}

[DataContract]
public class GetWorkflowVersionsResponse
{
    [DataMember(Order = 1)]
    public List<WorkflowVersion> Versions { get; set; } = new();
}

[DataContract]
public class WorkflowVersion
{
    [DataMember(Order = 1)]
    public string Id { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public int Version { get; set; }

    [DataMember(Order = 3)]
    public string Status { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public DateTime CreatedAt { get; set; }

    [DataMember(Order = 5)]
    public DateTime? PublishedAt { get; set; }
}

[DataContract]
public class GetCompiledWorkflowRequest
{
    [DataMember(Order = 1)]
    public string WorkflowVersionId { get; set; } = string.Empty;
}

[DataContract]
public class GetCompiledWorkflowResponse
{
    [DataMember(Order = 1)]
    public WorkflowVersionInfo Version { get; set; } = new();

    [DataMember(Order = 2)]
    public List<NodeInfo> Nodes { get; set; } = new();
}

[DataContract]
public class WorkflowVersionInfo
{
    [DataMember(Order = 1)]
    public string Id { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string DefinitionId { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public int Version { get; set; }

    [DataMember(Order = 4)]
    public string Status { get; set; } = string.Empty;
}

[DataContract]
public class NodeInfo
{
    [DataMember(Order = 1)]
    public string Id { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string Key { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string Name { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string Type { get; set; } = string.Empty;
}

[DataContract]
public class ValidateWorkflowVersionRequest
{
    [DataMember(Order = 1)]
    public string WorkflowVersionId { get; set; } = string.Empty;
}

[DataContract]
public class ValidationResultResponse
{
    [DataMember(Order = 1)]
    public bool IsValid { get; set; }

    [DataMember(Order = 2)]
    public List<string> Errors { get; set; } = new();

    [DataMember(Order = 3)]
    public List<string> Warnings { get; set; } = new();
}

