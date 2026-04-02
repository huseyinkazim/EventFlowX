using EventFlowX.Host.Data;
using Microsoft.EntityFrameworkCore;

namespace EventFlowX.Host.HostedService;

public class MigrationService(ILogger<MigrationService> logger,
    IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
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
