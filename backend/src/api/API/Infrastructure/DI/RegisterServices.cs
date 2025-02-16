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
        builder.AddSwaggerService();
        builder.AddCustomServices();
        builder.AddRateLimiterService();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddProblemDetails();
        builder.AddResponseCompressionService();

        return builder;
    }
}