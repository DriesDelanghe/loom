namespace Loom.Services.Configuration.Domain.Graph;

public class Connection
{
    public Guid Id { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public Guid FromNodeId { get; set; }
    public Guid ToNodeId { get; set; }
    public string Outcome { get; set; } = default!;
    public int? Order { get; set; }
}