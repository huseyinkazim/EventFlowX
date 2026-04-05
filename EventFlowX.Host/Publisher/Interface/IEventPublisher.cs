namespace EventFlowX.Host.Publisher.Interface;

public interface IEventPublisher
{
    Task PublishAsync(string queueName, string message, CancellationToken cancellationToken);
}