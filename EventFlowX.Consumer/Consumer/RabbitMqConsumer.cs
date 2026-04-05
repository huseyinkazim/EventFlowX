using EventFlowX.Consumer.Consumer.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EventFlowX.Consumer.Consumer;

public class RabbitMqConsumer : IEventSubscriber, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqConsumer(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<IEventSubscriber> CreateAsync()
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri("amqp://admin:admin@localhost:5672/")
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        return new RabbitMqConsumer(connection, channel);
    }

    public async Task SubscribeAsync(string queueName, Func<string, Task> onMessage, CancellationToken cancellationToken)
    {
        await _channel.QueueDeclareAsync(queueName, true, false, false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                await onMessage(message);
                await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
            }
            catch (Exception)
            {
                // işlenemedi → requeue et
                await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true, cancellationToken);
            }
        };

        await _channel.BasicConsumeAsync(queueName, autoAck: false, consumer, cancellationToken);

        // Cancellation bekle
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}