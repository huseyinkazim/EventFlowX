using EventFlowX.Host.Data;
using EventFlowX.Host.Helper;
using EventFlowX.Host.HostedService.Workers.Interfaces;
using EventFlowX.Host.Publisher.Interface;
using EventFlowX.Shared.Enums;
using EventFlowX.Shared.Models;
using EventFlowX.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EventFlowX.Host.HostedService.Workers;

public class PublisherWorker(
    ILogger<PublisherWorker> logger,
    IInstanceIdProvider instanceIdProvider,
    IServiceScopeFactory scopeFactory,
    IEventPublisher publisher) : IPublisherWorker
{
    private const int MaxRetryCount = 3;

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PublisherWorker started");

        await SetActivePotAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
            await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);

            var evts = await db.OutboxEvents
                .Where(x =>
                x.Status == EventStatus.Pending &&
                (string.IsNullOrEmpty(x.ProcessingBy) || x.ProcessingBy == instanceIdProvider.InstanceId))
                .OrderByDescending(x => x.ProcessingBy == instanceIdProvider.InstanceId)
                .ThenBy(x => x.CreatedAt)
                .Take(10)
                .ToListAsync(stoppingToken);

            if (evts.Count == 0)
            {
                await transaction.RollbackAsync(stoppingToken);
                await Task.Delay(2000, stoppingToken);
                continue;
            }

            foreach (var evt in evts)
            {
                evt.SetStatus(EventStatus.Processing);
                evt.SetProcessingBy(instanceIdProvider.InstanceId);
            }
            await db.SaveChangesAsync(stoppingToken);
            await transaction.CommitAsync(stoppingToken);

            foreach (var evt in evts)
                try
                {
                    logger.LogInformation("Processing event {EventId}", evt.Id);

                    await publisher.PublishAsync(evt.EventType, JsonSerializer.Serialize(evt.Data), stoppingToken);

                    evt.SetStatus(EventStatus.Processed);
                }
                catch (Exception ex)
                {
                    evt.RetryCount++;

                    if (RetryHelper.CanRetry(evt.RetryCount, MaxRetryCount))
                    {
                        evt.SetStatus(EventStatus.Pending);
                    }
                    else
                    {
                        evt.SetStatus(EventStatus.Failed);
                        evt.ErrorMessage = ex.Message;
                        logger.LogError(ex, "Error while publishing event {EventId}", evt.Id);
                    }
                }

            await db.SaveChangesAsync(stoppingToken);
        }
    }

    private async Task SetActivePotAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
                var existingPod = await db.Pods.FirstOrDefaultAsync(
                    i => i.InstanceId == instanceIdProvider.InstanceId, cancellationToken);

                if (existingPod != null)
                {
                    existingPod.Status = PodStatus.Running;
                    existingPod.HostName = Environment.MachineName;
                    var result = await db.OutboxEvents
                        .Where(e => e.ProcessingBy == existingPod.InstanceId &&
                        (e.Status == EventStatus.Processing || e.Status == EventStatus.Pending))
                        .ToListAsync(cancellationToken);

                    foreach (var e in result)
                    {
                        e.SetStatus(EventStatus.Pending);
                        e.SetProcessingBy(null);
                    }
                }
                else
                {
                    db.Pods.Add(new Pod
                    {
                        InstanceId = instanceIdProvider.InstanceId,
                        HostName = Environment.MachineName,
                        Status = PodStatus.Running
                    });
                }
                await db.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,"Pod Activation Error");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        var deadPod = await db.Pods.FirstAsync(i => i.InstanceId == instanceIdProvider.InstanceId && i.Status == PodStatus.Running);

        deadPod.Status = PodStatus.Stopping;

        await db.SaveChangesAsync();

        logger.LogInformation("PublisherWorker has stopped.{time}", DateTimeOffset.Now);
    }
}