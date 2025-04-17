namespace API.Infrastructure.Middlewares.TokenValidation;

/// <summary>
/// Middleware to validate tokens in incoming HTTP requests.
/// If the request is to an ignored URL, it bypasses token validation.
/// Otherwise, it validates the user's authentication status and token version.
/// </summary>
public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Constructor for TokenValidationMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="serviceScopeFactory">Factory to create service scopes for database access.</param>
    public TokenValidationMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Invokes the token validation logic for each HTTP request.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        string requestPath = context.Request.Path.ToString().ToLower().TrimEnd('/');

        // Skip token validation for URLs that are in the IgnoreUrls list (e.g., authentication routes)
        if (IgnoreUrl.IgnoreUrls.Contains(requestPath))
        {
            await _next(context);
            return;
        }

        // Proceed with token validation if the user is authenticated
        if (context.User.Identity is { IsAuthenticated: true })
        {
            await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
            DataContext dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

            string? userId = context.User.FindFirst(x => x.Type == CustomClaimTypes.Id)?.Value;
            string? tokenVersionClaim = context.User.FindFirst(x => x.Type == CustomClaimTypes.TokenVersion)?.Value;

            // Ensure userId and tokenVersion are present in the token
            if (userId is null || tokenVersionClaim is null)
            {
                await WriteErrorResponse(context, "Invalid token data");
                return;
            }

            // Retrieve the user from the database and validate the token version
            User? user = await dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user is null || user.TokenVersion.ToString() != tokenVersionClaim)
            {
                await WriteErrorResponse(context, "Invalid token version");
                return;
            }
        }

        // Continue with the next middleware if validation passes
        await _next(context);
    }

    /// <summary>
    /// Writes an error response when token validation fails.
    /// </summary>
    private static async Task WriteErrorResponse(HttpContext context, string message)
    {
        try
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            // Prepare the error response message
            object response = new { error = message };
            string jsonResponse = JsonConvert.SerializeObject(response);

            // Log the failure for monitoring purposes
            ILogger<TokenValidationMiddleware> logger =
                context.RequestServices.GetRequiredService<ILogger<TokenValidationMiddleware>>();
            logger.LogWarning("Token validation failed for path {RequestPath}: {Message}", context.Request.Path,
                message);

            // Write the error response to the HTTP response
            await context.Response.WriteAsync(jsonResponse);
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during error response generation
            ILogger<TokenValidationMiddleware> logger =
                context.RequestServices.GetRequiredService<ILogger<TokenValidationMiddleware>>();
            logger.LogError(ex, "Failed to write error response");
        }
    }
}