namespace EventFlowX.Shared.Queues;

public class QueueDefinition
{
    public required string Name { get; set; }
    public string RetryQueue => $"{Name}.retry";
    public string DeadQueue => $"{Name}.dead";
    public string DlxExchange => $"{Name}.dlx";
    public int RetryTtlMs { get; set; } = 5000;
}