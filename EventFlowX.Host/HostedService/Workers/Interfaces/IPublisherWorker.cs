namespace EventFlowX.Host.HostedService.Workers.Interfaces;

public interface IPublisherWorker : IWorker
{
    Task StopAsync(CancellationToken cancellationToken);
}
