using EventFlowX.Host.HostedService.Workers.Interfaces;

namespace EventFlowX.Host.HostedService;

public class HeartbeatService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var worker = scope.ServiceProvider.GetRequiredService<IHeartbeatWorker>();
        await worker.DoWorkAsync(stoppingToken);
    }
}