using EventFlowX.Workers.Workers.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventFlowX.Workers.Services;

public class PublisherService(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IPublisherWorker>()
                .DoWorkAsync(stoppingToken);
        }

        return Task.CompletedTask;
    }
}
