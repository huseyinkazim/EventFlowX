namespace EventFlowX.Shared.Models;

public record EventMessage(Guid EventId, string EventType, string Payload, DateTime OccurredAt);