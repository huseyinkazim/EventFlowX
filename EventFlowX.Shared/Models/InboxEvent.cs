using EventFlowX.Shared.Enums;

namespace EventFlowX.Shared.Models;

public class InboxEvent : AuditableEntity
{
    public Guid Id { get; set; }               // EventId → UNIQUE (idempotency key)
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public string? ProcessingBy { get; set; }   // INSTANCE_ID
    public string? ErrorMessage { get; set; }
}