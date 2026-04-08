namespace EventFlowX.Shared.Queues;

public static class QueueRegistry
{
    public static readonly QueueDefinition OrderCreated = new()
    {
        Name = "OrderCreated",
        RetryTtlMs = 5000
    };

    public static readonly List<QueueDefinition> All = new()
    {
        OrderCreated
    };
}