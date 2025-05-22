namespace API.Infrastructure.Middlewares.TokenValidation;

public class TokenValidationMiddleware(
    RequestDelegate next,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<TokenValidationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            string requestPath = context.Request.Path.ToString().ToLower().TrimEnd('/');

            if (IgnoreUrl.IgnoreUrls.Contains(requestPath))
            {
                await next(context);
                return;
            }

            if (context.User.Identity?.IsAuthenticated != true)
            {
                await WriteErrorResponse(context, "Unauthorized: user is not authenticated.");
                return;
            }

            var userId = context.User.FindFirst(x => x.Type == CustomClaimTypes.Id)?.Value;
            var tokenVersionClaim = context.User.FindFirst(x => x.Type == CustomClaimTypes.TokenVersion)?.Value;

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tokenVersionClaim))
            {
                await WriteErrorResponse(context, "Invalid token claims.");
                return;
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                await WriteErrorResponse(context, "Invalid user ID format.");
                return;
            }

            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

            var user = await dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userGuid);

            if (user == null)
            {
                await WriteErrorResponse(context, "User not found.");
                return;
            }

            if (user.TokenVersion.ToString() != tokenVersionClaim)
            {
                await WriteErrorResponse(context, "Token version mismatch.");
                return;
            }

            await next(context); // Всё ок, продолжаем pipeline
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception in TokenValidationMiddleware");
            await WriteErrorResponse(context, "Unexpected error occurred during token validation.");
        }
    }

    private async Task WriteErrorResponse(HttpContext context, string message)
    {
        try
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var json = JsonConvert.SerializeObject(new { error = message });
            logger.LogWarning("Auth failed: {Message} — {Path}", message, context.Request.Path);
            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write error response.");
        }
    }
}
