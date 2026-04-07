using EventFlowX.Host.Data;
using EventFlowX.Host.HostedService.Workers.Interfaces;
using EventFlowX.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace EventFlowX.Host.HostedService.Workers;

public class HeartbeatWorker(
    ILogger<HeartbeatWorker> logger,
    IInstanceIdProvider instanceIdProvider,
    IServiceScopeFactory scopeFactory) : IHeartbeatWorker
{
    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

                await db.Pods
                    .Where(p => p.InstanceId == instanceIdProvider.InstanceId)
                    .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.LastHeartbeat, DateTime.UtcNow), stoppingToken);

                logger.LogDebug("Heartbeat sent. {InstanceId}", instanceIdProvider.InstanceId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Heartbeat failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}