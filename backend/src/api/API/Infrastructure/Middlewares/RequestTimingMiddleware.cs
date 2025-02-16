namespace API.Infrastructure.Middlewares;

public class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        long startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            await next(context);
        }
        finally
        {
            long endTimestamp = Stopwatch.GetTimestamp();
            double elapsedMs = GetElapsedMilliseconds(startTimestamp, endTimestamp);

            logger.LogInformation("⏳ Request {Method} {Path} completed in {Time} ms",
                context.Request.Method, context.Request.Path, elapsedMs);
        }
    }

    private static double GetElapsedMilliseconds(long start, long end)
    {
        return (end - start) * 1000.0 / Stopwatch.Frequency;
    }
}