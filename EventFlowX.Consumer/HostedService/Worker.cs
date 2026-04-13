using EventFlowX.Consumer.Data;
using EventFlowX.Infra.Messaging.Interface;
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
        try
        {
            // Subscribe both handlers properly
            await subscriber.SubscribeAsync("OrderCreated",
                message => HandleMainAsync(message, stoppingToken),
                stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Worker service is stopping...");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Fatal error in worker service");
            throw;
        }
    }

    private async Task HandleMainAsync(string message, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InboxDbContext>();

            var eventMessage = JsonSerializer.Deserialize<EventMessage>(message);
            if (eventMessage is null)
            {
                logger.LogError("Failed to deserialize message: {Message}", message);
                return;
            }

            var exists = await db.InboxEvents.AnyAsync(
                i => i.Id == eventMessage.EventId,
                stoppingToken);

            if (exists)
            {
                logger.LogInformation("Duplicate event {EventId}, skipping.", eventMessage.EventId);
                return;
            }

            await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
            try
            {
                var inboxEvent = new InboxEvent
                {
                    Id = eventMessage.EventId,
                    EventType = eventMessage.EventType,
                    Payload = eventMessage.Payload,
                    ProcessingBy = instanceIdProvider.InstanceId,
                    Status = EventStatus.Processing,
                };

                db.InboxEvents.Add(inboxEvent);
                await db.SaveChangesAsync(stoppingToken);

                await ExecuteBusinessLogicAsync(inboxEvent, eventMessage, stoppingToken);

                inboxEvent.Status = EventStatus.Processed;
                await db.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);

                logger.LogInformation("Successfully processed event {EventId}", eventMessage.EventId);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                await transaction.RollbackAsync(stoppingToken);
                logger.LogInformation("Duplicate event {EventId} caught by constraint.", eventMessage.EventId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(stoppingToken);
                logger.LogError(ex, "Error processing event {EventId}", eventMessage.EventId);

                await StoreFailedEventAsync(db, eventMessage, ex, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in HandleMainAsync");
        }
    }

    private async Task ExecuteBusinessLogicAsync(
        InboxEvent inboxEvent,
        EventMessage eventMessage,
        CancellationToken stoppingToken)
    {
        // TODO: Implement business logic based on event type
        logger.LogInformation("Executing business logic for event {EventId} of type {EventType}",
            eventMessage.EventId, eventMessage.EventType);

        await Task.CompletedTask;
    }

    private async Task StoreFailedEventAsync(
        InboxDbContext db,
        EventMessage eventMessage,
        Exception ex,
        CancellationToken stoppingToken)
    {
        try
        {
            var failedEvent = new InboxEvent
            {
                Id = eventMessage.EventId,
                EventType = eventMessage.EventType,
                Payload = eventMessage.Payload,
                Status = EventStatus.Failed,
                ErrorMessage = ex.Message
            };

            db.InboxEvents.Add(failedEvent);
            await db.SaveChangesAsync(stoppingToken);
        }
        catch (Exception saveEx)
        {
            logger.LogError(saveEx, "Failed to store failed event {EventId}", eventMessage.EventId);
        }
    }
}
