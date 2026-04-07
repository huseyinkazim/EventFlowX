using EventFlowX.Shared.Enums;

namespace EventFlowX.Shared.Models;

public class OutboxEvent : AuditableEntity
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = null!;
    public EventMessage Data { get; set; } = null!;
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
    public void SetData(Guid EventId, string EventType, string Payload, DateTime OccurredAt)
    {
        Id= EventId;
        Data = new EventMessage(EventId, EventType, Payload, OccurredAt);
    }
}