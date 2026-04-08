namespace EventFlowX.Infra.Messaging.Interface;

public interface IEventSubscriber
{
    Task SubscribeAsync(string queueName, Func<string, Task> onMessage, CancellationToken cancellationToken);
}