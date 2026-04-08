using EventFlowX.Infra.Messaging.Interface;
using RabbitMQ.Client;
using System.Text;

namespace EventFlowX.Infra.Messaging;


public class RabbitMqPublisher : IEventPublisher
{
    private readonly IRabbitMqConnection _connectionProvider;

    public RabbitMqPublisher(IRabbitMqConnection connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task PublishAsync(string queueName, string message, CancellationToken cancellationToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            body: body,
            cancellationToken: cancellationToken);
    }

}