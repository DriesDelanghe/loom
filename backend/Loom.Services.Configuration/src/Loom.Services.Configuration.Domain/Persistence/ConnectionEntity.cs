using Loom.Services.Configuration.Domain.Graph;

namespace Loom.Services.Configuration.Domain.Persistence;

public class ConnectionEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowVersionId { get; set; }
    public Guid FromNodeId { get; set; }
    public Guid ToNodeId { get; set; }
    public string Outcome { get; set; } = default!;
    public int? Order { get; set; }

    public WorkflowVersionEntity WorkflowVersion { get; set; } = null!;

    public Connection ToDomain()
    {
        return new Connection
        {
            Id = Id,
            WorkflowVersionId = WorkflowVersionId,
            FromNodeId = FromNodeId,
            ToNodeId = ToNodeId,
            Outcome = Outcome,
            Order = Order
        };
    }

    public static ConnectionEntity FromDomain(Connection connection)
    {
        return new ConnectionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = connection.WorkflowVersionId,
            FromNodeId = connection.FromNodeId,
            ToNodeId = connection.ToNodeId,
            Outcome = connection.Outcome,
            Order = connection.Order
        };
    }
}


