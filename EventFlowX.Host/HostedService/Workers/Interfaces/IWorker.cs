namespace EventFlowX.Host.HostedService.Workers.Interfaces;

public interface IWorker
{
    Task DoWorkAsync(CancellationToken stoppingToken);
}
