using EventFlowX.Shared.Enums;

namespace EventFlowX.Shared.Models;

public class OutboxEvent : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public EventStatus Status { get; private set; } = EventStatus.Pending;
    public string? ProcessingBy { get; private set; }
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public virtual Pod? Pod { get; set; }
    public void SetStatus(EventStatus eventStatus)
    {
        Status = eventStatus;
    }
    public void SetPod(Pod pod)
    {
        Pod = pod;
    }
    public void SetProcessingBy(string? processingBy)
    {
        ProcessingBy = processingBy;
    }
}