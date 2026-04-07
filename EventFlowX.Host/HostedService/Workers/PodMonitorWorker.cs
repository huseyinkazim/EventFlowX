using EventFlowX.Host.Data;
using EventFlowX.Host.HostedService.Workers.Interfaces;
using EventFlowX.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventFlowX.Host.HostedService.Workers;

public class PodMonitorWorker(
    ILogger<PodMonitorWorker> logger,
    IServiceScopeFactory scopeFactory) : IPodMonitorWorker
{
    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

                var staleThreshold = DateTime.UtcNow.AddSeconds(-90);
                var stalePods = await db.Pods
                    .Where(p => p.Status == PodStatus.Running && p.LastHeartbeat < staleThreshold)
                    .ToListAsync(stoppingToken);

                foreach (var stalePod in stalePods)
                {
                    stalePod.Status = PodStatus.Dead;

                    var staleEvents = await db.OutboxEvents
                        .Where(e => e.ProcessingBy == stalePod.InstanceId &&
                               (e.Status == EventStatus.Processing || e.Status == EventStatus.Pending))
                        .ToListAsync(stoppingToken);

                    foreach (var e in staleEvents)
                    {
                        e.SetStatus(EventStatus.Pending);
                        e.SetProcessingBy(null);
                    }

                    logger.LogWarning("Pod marked as Dead. {InstanceId}", stalePod.InstanceId);
                }

                if (stalePods.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PodMonitorWorker failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}