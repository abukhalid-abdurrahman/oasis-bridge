namespace API.Infrastructure.DI;

/// <summary>
/// This class registers rate-limiting services to the application's dependency injection container.
/// It ensures that requests to the API are limited based on the client's IP address and the specific route being accessed.
/// </summary>
public static class RateLimiterRegister
{
    private static readonly TimeSpan
        WindowSize = TimeSpan.FromMinutes(1); // The duration of the time window for rate-limiting.

    private const int RequestLimit = 20; // The maximum number of requests allowed within the specified window.

    /// <summary>
    /// Registers the rate-limiting service for the application to limit the number of requests that clients can make.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance to register services with.</param>
    /// <returns>The WebApplicationBuilder instance with the rate-limiting services registered.</returns>
    public static WebApplicationBuilder AddRateLimiterService(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            // Configure the global rate limiter to be partitioned based on the client's IP address and requested route.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                // Get the client's IP address and the requested route path.
                string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "UnknownIP";
                string routePath = httpContext.Request.Path.Value ?? "/";

                // Create a partition key based on the combination of IP address and route path.
                string partitionKey = $"{ipAddress}:{routePath}";

                // Define the rate limit for each partition using a fixed window model.
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        // Set the number of requests allowed within the time window.
                        PermitLimit = RequestLimit,
                        // Set the time window duration.
                        Window = WindowSize,
                        // Set the queue limit to zero, meaning no queuing of requests after the limit is reached.
                        QueueLimit = 0,
                        // Enable automatic replenishment of available requests.
                        AutoReplenishment = true
                    });
            });

            // Set the HTTP status code to return when the rate limit is exceeded.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return builder;
    }
}