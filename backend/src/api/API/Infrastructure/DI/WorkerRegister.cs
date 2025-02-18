namespace API.Infrastructure.DI;

public static class WorkerRegister
{
    public static WebApplicationBuilder AddWorkerService(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<ExchangeRateWorker>();
        builder.Services.AddScoped<ExchangeRateUpdaterService>();
        return builder;
    }
}