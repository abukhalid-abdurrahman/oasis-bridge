namespace API.Infrastructure.DI;

/// <summary>
/// This class registers various services necessary for the application to function properly.
/// It integrates a wide range of services into the application's dependency injection container
/// to enable essential functionality such as authentication, database access, email communication, 
/// rate limiting, and more.
/// </summary>
public static class RegisterServices
{
    /// <summary>
    /// Registers various services needed for the application.
    /// This method configures and registers services related to authentication, 
    /// HTTP handling, database connectivity, email services, and other essential functionalities.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance to register services with.</param>
    /// <returns>The WebApplicationBuilder instance with services registered.</returns>
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        // Register database-related services
        builder.AddDbService();
        // Register JWT authentication services
        builder.AddJwtService();
        // Register HTTP client and context services
        builder.AddHttpService();
        // Register CORS services for cross-origin resource sharing
        builder.AddCorsService();
        // Register Ipfs services
        builder.AddIpfsService();
        // Register email-related services
        builder.AddEmailService();
        // Register bridge-specific services (likely for specific business logic)
        builder.AddBridgeService();
        // Register background worker services (for periodic tasks)
        builder.AddWorkerService();
        // Register custom business logic services like roles, users, and orders
        builder.AddCustomServices();
        // Register Swagger for API documentation generation
        builder.AddSwaggerService();
        // Register rate limiting services to control request flow
        builder.AddRateLimiterService();
        // Add Swagger generation configuration
        builder.Services.AddSwaggerGen();
        // Add MVC controllers support for the API
        builder.Services.AddControllers();
        // Add ProblemDetails to handle errors and problems in a standardized way
        builder.Services.AddProblemDetails();
        // Register response compression services for efficient data transfer
        builder.AddResponseCompressionService();
        // Add service  for Convert enum value 
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false)
                );
            });

        builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 2073741824; });

        return builder;
    }
}