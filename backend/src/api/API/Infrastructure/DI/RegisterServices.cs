namespace API.Infrastructure.DI;

public static class RegisterServices
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.AddDbService();
        builder.AddJwtService();
        builder.AddHttpService();
        builder.AddCorsService();
        builder.AddEmailService();
        builder.AddBridgeService();
        builder.AddWorkerService();
        builder.AddCustomServices();
        builder.AddSwaggerService();
        builder.AddRateLimiterService();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddProblemDetails();
        builder.AddResponseCompressionService();

        return builder;
    }
}