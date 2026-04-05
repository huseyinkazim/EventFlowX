using EventFlowX.Consumer.Consumer.Interface;

namespace EventFlowX.Consumer.HostedService;

public class Worker(ILogger<Worker> logger,
    IEventSubscriber subscriber) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await subscriber.SubscribeAsync("OrderCreated", async message =>
            {
                // inbox kontrolü + business logic
            }, stoppingToken);
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await Task.Delay(1000, stoppingToken);
        }
    }
}
