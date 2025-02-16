namespace API.Infrastructure.Middlewares;

public class RequestCancellationMiddleware(RequestDelegate next, ILogger<RequestCancellationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        string method = context.Request.Method;
        string path = context.Request.Path;
        string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        context.Items["RequestCancellationSource"] = cts;

        try
        {
            logger.LogInformation("➡ Request started: {Method} {Path} | IP: {IpAddress}", method, path, ipAddress);

            await next(context);

            stopwatch.Stop();
            logger.LogInformation(
                "✅ Request completed: {Method} {Path} | Status: {StatusCode} | Duration: {ElapsedMs} ms",
                method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            logger.LogWarning("⚠ Request canceled: {Method} {Path} | Duration: {ElapsedMs} ms",
                method, path, stopwatch.ElapsedMilliseconds);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "❌ Request failed: {Method} {Path} | Duration: {ElapsedMs} ms",
                method, path, stopwatch.ElapsedMilliseconds);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
        finally
        {
            if (!cts.IsCancellationRequested)
            {
                logger.LogInformation("🛑 Request cancellation triggered: {Method} {Path}", method, path);
                await cts.CancelAsync();
            }
        }
    }
}