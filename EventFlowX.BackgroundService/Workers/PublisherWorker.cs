using EventFlowX.Workers.Workers.Interface;
using Microsoft.Extensions.Logging;

namespace EventFlowX.Workers.Workers;

public class PublisherWorker(ILogger<PublisherWorker> logger) : IPublisherWorker
{
    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"PublisherWorker is started at {DateTimeOffset.Now}");


        stoppingToken.Register(() =>
        {
            logger.LogInformation("PublisherWorker has stopped.{time}", DateTimeOffset.Now);
        });
        //todo:wait 1 minute then check events if there is event send message to rabbitmq

        while (!stoppingToken.IsCancellationRequested)
        {
            //Status: Pending | Processing | Processed | Failed
            //çift consumer çalışabileceği için çalışan pod için processingby diyip diğer pod için çalışamaz yapabiliriz. Böylece aynı event'i iki kere işleme riskini ortadan kaldırmış oluruz.
            //if()//event varsa
            //logger.LogInformation($"PublisherWorker service running.");
            //else
            //await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            try
            {
                //send message to rabbitmq
                //logger.LogInformation($"PublisherWorker service has finished.");

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An error occurred while the PublisherWorker service was running.");
            }
        }
    }
}
