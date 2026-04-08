namespace EventFlowX.Infra.Messaging.Interface;

public interface IEventPublisher
{
    Task PublishAsync(string queueName, string message, CancellationToken cancellationToken);
}