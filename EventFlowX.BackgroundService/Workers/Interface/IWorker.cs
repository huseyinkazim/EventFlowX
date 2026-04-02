namespace EventFlowX.Workers.Workers.Interface;

public interface IWorker
{
    Task DoWorkAsync(CancellationToken stoppingToken);
}
