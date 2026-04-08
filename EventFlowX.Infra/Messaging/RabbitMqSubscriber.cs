using EventFlowX.Infra.Messaging.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EventFlowX.Infra.Messaging;

public class RabbitMqSubscriber : IEventSubscriber
{
    private readonly IRabbitMqConnection _connectionProvider;
    private const int MaxRetryCount = 3;

    public RabbitMqSubscriber(IRabbitMqConnection connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task SubscribeAsync(
        string queueName,
        Func<string, Task> onMessage,
        CancellationToken cancellationToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        await channel.BasicQosAsync(0, 1, false);
        var dlxExchange = "dlx";
        var retryExchange = "retry-exchange";

        var deadQueue = $"{queueName}.dead";
        var retryQueue = $"{queueName}.retry";

        #region Exchanges

        await channel.ExchangeDeclareAsync(dlxExchange, ExchangeType.Direct, durable: true);
        await channel.ExchangeDeclareAsync(retryExchange, ExchangeType.Direct, durable: true);

        #endregion

        #region Dead Queue

        await channel.QueueDeclareAsync(deadQueue, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(deadQueue, dlxExchange, queueName);

        #endregion

        #region Retry Queue

        var retryArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", "" }, // default exchange
            { "x-dead-letter-routing-key", queueName },
            { "x-message-ttl", 10000 } // 10 sn sonra geri dön
        };

        await channel.QueueDeclareAsync(retryQueue, durable: true, exclusive: false, autoDelete: false, arguments: retryArgs);

        await channel.QueueBindAsync(retryQueue, retryExchange, queueName);

        #endregion

        #region Main Queue

        var mainArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", dlxExchange },
            { "x-dead-letter-routing-key", queueName }
        };

        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: mainArgs);

        #endregion

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                await onMessage(message);

                await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
            }
            catch
            {
                var retryCount = GetRetryCount(ea);

                if (retryCount >= MaxRetryCount)
                {
                    // DLQ'ya gönder
                    await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false, cancellationToken);
                }
                else
                {
                    // Retry queue'ya publish et
                    var props = new BasicProperties();
                    props.Headers = ea.BasicProperties.Headers ?? new Dictionary<string, object?>();

                    props.Headers["x-retry-count"] = retryCount + 1;

                    await channel.BasicPublishAsync(
                        exchange: retryExchange,
                        routingKey: queueName,
                        mandatory: false,
                        basicProperties: props,
                        body: ea.Body);

                    await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                }
            }
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
    }

    private int GetRetryCount(BasicDeliverEventArgs ea)
    {
        if (ea.BasicProperties?.Headers == null)
            return 0;

        if (ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var value))
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch { }
        }

        return 0;
    }
}