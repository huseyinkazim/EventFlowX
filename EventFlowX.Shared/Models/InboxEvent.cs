using EventFlowX.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlowX.Shared.Models;

public class InboxEvent
{
    public Guid Id { get; set; }               // EventId → UNIQUE (idempotency key)
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public EventStatus Status { get; set; } = EventStatus.Pending;
    public string? ProcessingBy { get; set; }   // INSTANCE_ID
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
}