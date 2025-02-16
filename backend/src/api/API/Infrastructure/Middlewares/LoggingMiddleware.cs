namespace API.Infrastructure.Middlewares;

public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        string request = await FormatRequest(context.Request);
        logger.LogInformation("➡️ Incoming Request: {Request}", request);

        Stopwatch stopwatch = Stopwatch.StartNew();

        Stream originalBodyStream = context.Response.Body;
        await using MemoryStream responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await next(context);

        stopwatch.Stop();
        string response = await FormatResponse(context.Response);
        logger.LogInformation("⬅️ Outgoing Response ({Time}ms): {Response}", stopwatch.ElapsedMilliseconds, response);

        await responseBody.CopyToAsync(originalBodyStream);
    }

    private async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();
        Stream body = request.Body;
        byte[] buffer = new byte[Convert.ToInt32(request.ContentLength)];
        int readAsync = await request.Body.ReadAsync(buffer, 0, buffer.Length);
        request.Body.Position = 0;
        return $"Method: {request.Method}, Path: {request.Path}, Body: {Encoding.UTF8.GetString(buffer)}";
    }

    private async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        string text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        return $"Status: {response.StatusCode}, Body: {text}";
    }
}