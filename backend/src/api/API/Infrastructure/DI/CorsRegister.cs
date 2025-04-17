namespace API.Infrastructure.DI;

/// <summary>
/// Provides an extension method to register CORS services in the application's DI container.
/// </summary>
public static class CorsRegister
{
    /// <summary>
    /// Adds CORS services to the WebApplicationBuilder, enabling cross-origin requests.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance.</param>
    /// <returns>The WebApplicationBuilder instance with CORS services registered.</returns>
    public static WebApplicationBuilder AddCorsService(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin() // Allows requests from any origin (domain).
                    .AllowAnyMethod() // Allows any HTTP method (GET, POST, PUT, DELETE, etc.).
                    .AllowAnyHeader(); // Allows any header to be sent in the request.
            });
        });
        return builder;
    }
}