using EventFlowX.Infra.Messaging.Interface;
using RabbitMQ.Client;

namespace EventFlowX.Infra.Messaging;

public class RabbitMqConnection : IRabbitMqConnection
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitMqConnection(string uri)
    {
        _factory = new ConnectionFactory
        {
            Uri = new Uri(uri)
        };
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync();
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            _connection = await _factory.CreateConnectionAsync();
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}