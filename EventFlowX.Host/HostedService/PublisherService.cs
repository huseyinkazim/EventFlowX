using EventFlowX.Host.HostedService.Workers.Interfaces;

namespace EventFlowX.Workers.Services;

public class PublisherService(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<IPublisherWorker>()
                .DoWorkAsync(stoppingToken); 
        }
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IPublisherWorker>()
                   .StopAsync(cancellationToken);
        }

        return base.StopAsync(cancellationToken);
    }
}
