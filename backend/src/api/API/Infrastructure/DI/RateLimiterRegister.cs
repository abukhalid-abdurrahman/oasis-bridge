namespace API.Infrastructure.DI;

public static class RateLimiterRegister
{
    private static readonly TimeSpan WindowSize = TimeSpan.FromMinutes(1);
    private const int RequestLimit = 20;

    public static WebApplicationBuilder AddRateLimiterService(this WebApplicationBuilder builder)
    {
        // builder.Services.AddRateLimiter(options =>
        // {
        //     options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        //     {
        //         string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "UnknownIP";
        //         string routePath = httpContext.Request.Path.Value ?? "/";
        //
        //         string partitionKey = $"{ipAddress}:{routePath}";
        //
        //         return RateLimitPartition.GetFixedWindowLimiter(
        //             partitionKey,
        //             _ => new FixedWindowRateLimiterOptions
        //             {
        //                 PermitLimit = RequestLimit,
        //                 Window = WindowSize,
        //                 QueueLimit = 0,
        //                 AutoReplenishment = true
        //             });
        //     });
        //
        //     options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        // });

        return builder;
    }
}