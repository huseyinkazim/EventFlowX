using EventFlowX.Consumer.Consumer.Interface;
using EventFlowX.Consumer.Data;
using EventFlowX.Shared.Enums;
using EventFlowX.Shared.Models;
using EventFlowX.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EventFlowX.Consumer.HostedService;

public class Worker(ILogger<Worker> logger,
    IEventSubscriber subscriber,
    IInstanceIdProvider instanceIdProvider,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await subscriber.SubscribeAsync("OrderCreated", async message =>
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InboxDbContext>();

            var eventMessage = JsonSerializer.Deserialize<EventMessage>(message);
            if (eventMessage is null)
            {
                logger.LogError("Failed to deserialize message: {Message}", message);
                return;
            }

            await using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var exists = await db.InboxEvents.AnyAsync(i => i.Id == eventMessage.EventId);
                if (exists)
                {
                    logger.LogInformation("Duplicate event {EventId}, skipping.", eventMessage.EventId);
                    return;
                }

                var inboxEvent = new InboxEvent
                {
                    Id = eventMessage.EventId,
                    EventType = eventMessage.EventType,
                    Payload = eventMessage.Payload,
                    ProcessingBy = instanceIdProvider.InstanceId,
                    Status = EventStatus.Processing,
                };
                db.InboxEvents.Add(inboxEvent);
                await db.SaveChangesAsync();
                #region BussinessLogic
                // Business logic buraya
                #endregion
                logger.LogInformation("Processing event {EventId}", eventMessage.EventId);

                inboxEvent.Status = EventStatus.Processed;

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                await transaction.RollbackAsync();
                logger.LogInformation("Duplicate event {EventId} caught by constraint.", eventMessage.EventId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Failed to process event {EventId}", eventMessage.EventId);
                throw;
            }
        }, stoppingToken);
    }
}
