namespace EventFlowX.Consumer.Consumer.Interface;

public interface IEventSubscriber
{
    Task SubscribeAsync(string queueName, Func<string, Task> onMessage, CancellationToken cancellationToken);
}