using EventFlowX.Host.Publisher.Interface;
using RabbitMQ.Client;
using System.Text;

public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqPublisher(IConnection connection,IChannel channel) 
    {
        _connection=connection; 
        _channel=channel;
    }

    public static async Task<IEventPublisher> CreateAsync()
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri("amqp://admin:admin@localhost:5672/")
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        return new RabbitMqPublisher(connection, channel);
    }
    public async Task PublishAsync(string queueName, string message, CancellationToken cancellationToken)
    {
        await _channel.QueueDeclareAsync(queueName, true, false, false);

        var body = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            body: body,
            cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}