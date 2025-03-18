namespace API.Infrastructure.DI;

public static class CorsRegister
{
    public static WebApplicationBuilder AddCorsService(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
        return builder;
    }
}