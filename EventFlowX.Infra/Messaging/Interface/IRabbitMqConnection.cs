using RabbitMQ.Client;

namespace EventFlowX.Infra.Messaging.Interface;

public interface IRabbitMqConnection : IDisposable
{
    Task<IConnection> GetConnectionAsync();
}