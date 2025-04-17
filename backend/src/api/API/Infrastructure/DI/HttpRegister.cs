namespace API.Infrastructure.DI;

/// <summary>
/// This class registers HTTP-related services within the DI container for the application.
/// It configures essential services for HTTP requests, including HttpClient, HttpContextAccessor, and HTTP request logging.
/// </summary>
public static class HttpRegister
{
    /// <summary>
    /// Registers HTTP-related services in the DI container.
    /// This includes setting up the HttpClient for making HTTP requests, HttpContextAccessor for accessing HTTP context,
    /// and enabling HTTP logging for monitoring and debugging HTTP requests.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance to register services with.</param>
    /// <returns>The WebApplicationBuilder instance with HTTP-related services registered.</returns>
    public static WebApplicationBuilder AddHttpService(this WebApplicationBuilder builder)
    {
        // Registering HttpClient service for making outbound HTTP requests.
        builder.Services.AddHttpClient();

        // Registering HttpContextAccessor to allow accessing the current HTTP request context.
        builder.Services.AddHttpContextAccessor();

        // Registering HTTP logging service to log all HTTP request and response data.
        builder.Services.AddHttpLogging(options =>
        {
            // Configuring the logging options to capture all HTTP request and response details.
            options.LoggingFields = HttpLoggingFields.All;
        });

        // Returning the builder for method chaining.
        return builder;
    }
}