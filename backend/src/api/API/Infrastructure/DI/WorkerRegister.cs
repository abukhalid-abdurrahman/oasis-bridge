namespace API.Infrastructure.DI;

/// <summary>
/// This class provides an extension method for configuring background worker services in a WebApplicationBuilder.
/// It registers the services required for performing background tasks, such as updating exchange rates at regular intervals.
/// </summary>
public static class WorkerRegister
{
    /// <summary>
    /// Registers worker services for background tasks in the application.
    /// This includes registering the `ExchangeRateWorker` as a hosted service that runs in the background,
    /// and the `ExchangeRateUpdaterService` to update exchange rates periodically.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance used to register services.</param>
    /// <returns>The WebApplicationBuilder instance to allow method chaining.</returns>
    public static WebApplicationBuilder AddWorkerService(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<ExchangeRateWorker>();
        builder.Services.AddScoped<ExchangeRateUpdaterService>();
        return builder;
    }
}