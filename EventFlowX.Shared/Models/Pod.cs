using EventFlowX.Shared.Enums;

namespace EventFlowX.Shared.Models;

public class Pod : AuditableEntity
{
    public string InstanceId { get; set; } = null!;  // PK — unique
    public string HostName { get; set; } = null!;     // Environment.MachineName
    public PodStatus Status { get; set; }             // Running | Stopping | Dead
}