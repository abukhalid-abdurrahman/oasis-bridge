namespace API.Infrastructure.Workers.ExchangeRate;

public class ExchangeRateWorker(
    ILogger<ExchangeRateWorker> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5);

    private const int Count = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExchangeRateWorker started.");
        logger.LogInformation("-----------------------------------------------------------------------------------");
        logger.LogInformation("-----------------------------------------------------------------------------------");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = serviceScopeFactory.CreateScope();
                ExchangeRateUpdaterService exchangeRateUpdater =
                    scope.ServiceProvider.GetRequiredService<ExchangeRateUpdaterService>();
                await exchangeRateUpdater.UpdateExchangeRatesAsync(stoppingToken);
                logger.LogInformation($"Completed UpdateExchangeRatesAsync-{Count} ");
                logger.LogInformation(
                    "-----------------------------------------------------------------------------------");
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