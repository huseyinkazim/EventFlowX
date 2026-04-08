using EventFlowX.Infra.Messaging.Interface;
using EventFlowX.Shared.Queues;
using RabbitMQ.Client;

namespace EventFlowX.Host.HostedService;

public class RabbitMqInitializer : IHostedService
{
    private readonly IRabbitMqConnection _connectionProvider;

    public RabbitMqInitializer(IRabbitMqConnection connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        foreach (var queue in QueueRegistry.All)
            await CreateQueue(queue, channel, cancellationToken);
    }

    private async Task CreateQueue(QueueDefinition queue, IChannel channel, CancellationToken cancellationToken)
    {
        // 1. DEAD LETTER EXCHANGE
        await channel.ExchangeDeclareAsync(
            exchange: queue.DlxExchange,
            type: ExchangeType.Direct,
            durable: true,
            cancellationToken: cancellationToken);

        // 2. MAIN QUEUE (DLX'e bağlı)
        await channel.QueueDeclareAsync(
            queue: queue.Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", queue.DlxExchange }
            },
            cancellationToken: cancellationToken);

        // DLX binding (retry ve dead routing)
        await channel.QueueBindAsync(
            queue: queue.RetryQueue,
            exchange: queue.DlxExchange,
            routingKey: queue.RetryQueue,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: queue.DeadQueue,
            exchange: queue.DlxExchange,
            routingKey: queue.DeadQueue,
            cancellationToken: cancellationToken);

        // 3. RETRY QUEUE (TTL → back to main queue)
        await channel.QueueDeclareAsync(
            queue: queue.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                { "x-message-ttl", 5000 },
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", queue.Name }
            },
            cancellationToken: cancellationToken);

        // 4. DEAD LETTER QUEUE
        await channel.QueueDeclareAsync(
            queue: queue.DeadQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}