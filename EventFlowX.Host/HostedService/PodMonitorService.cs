using EventFlowX.Host.HostedService.Workers.Interfaces;

namespace EventFlowX.Host.HostedService;

public class PodMonitorService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var worker = scope.ServiceProvider.GetRequiredService<IPodMonitorWorker>();
        await worker.DoWorkAsync(stoppingToken);
    }
}