namespace API.Infrastructure.Workers.ExchangeRate;

public class ExchangeRateWorker(ExchangeRateUpdaterService service, ILogger<ExchangeRateWorker> logger)
    : BackgroundService
{
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5);

    static readonly int count = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExchangeRateWorker started.");
        logger.LogInformation("-----------------------------------------------------------------------------------");
        logger.LogInformation("-----------------------------------------------------------------------------------");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await service.UpdateExchangeRatesAsync(stoppingToken);
                logger.LogInformation($"Completed UpdateExchangeRatesAsync-{count} ");
                logger.LogInformation("-----------------------------------------------------------------------------------");
                logger.LogInformation(
                    "-----------------------------------------------------------------------------------");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating exchange rates.");
                logger.LogError("-----------------------------------------------------------------------------------");
                logger.LogError("-----------------------------------------------------------------------------------");
            }

            await Task.Delay(_updateInterval, stoppingToken);
        }
    }
}