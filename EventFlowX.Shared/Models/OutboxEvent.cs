using EventFlowX.Shared.Shared;

namespace EventFlowX.Shared.Models;

public class OutboxEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public string? ProcessingBy { get; set; }   // INSTANCE_ID
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
}