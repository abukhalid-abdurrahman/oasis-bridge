namespace API.Infrastructure.Middlewares.TokenValidation;

public class TokenValidationMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string requestPath = context.Request.Path.ToString().ToLower().TrimEnd('/');
        if (IgnoreUrl.IgnoreUrls.Contains(requestPath))
        {
            await next(context);
            return;
        }

        if (context.User.Identity is { IsAuthenticated: true })
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            DataContext dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

            string? userId = context.User.FindFirst(x => x.Type == CustomClaimTypes.Id)?.Value;
            string? tokenVersionClaim = context.User.FindFirst(x => x.Type == CustomClaimTypes.TokenVersion)?.Value;

            if (userId is null || tokenVersionClaim is null)
            {
                await WriteErrorResponse(context, "Invalid token data");
                return;
            }

            User? user = await dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user is null || user.TokenVersion.ToString() != tokenVersionClaim)
            {
                await WriteErrorResponse(context, "Invalid token version");
                return;
            }
        }

        await next(context);
    }

    private static async Task WriteErrorResponse(HttpContext context, string message)
    {
        try
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            object response = new { error = message };
            string jsonResponse = JsonConvert.SerializeObject(response);

            ILogger<TokenValidationMiddleware> logger =
                context.RequestServices.GetRequiredService<ILogger<TokenValidationMiddleware>>();
            logger.LogWarning("Token validation failed for path {RequestPath}: {Message}", context.Request.Path,
                message);

            await context.Response.WriteAsync(jsonResponse);
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<TokenValidationMiddleware>>();
            logger.LogError(ex, "Failed to write error response");
        }
    }
}