using EventFlowX.Consumer.Data;
using Microsoft.EntityFrameworkCore;

namespace EventFlowX.Consumer.HostedService;

public class MigrationService(ILogger<MigrationService> logger,
    IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {


        using (var scope = serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InboxDbContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migration completed.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Database migration finished.");

        return Task.CompletedTask;
    }
}