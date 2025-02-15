namespace API.Infrastructure.DI;

public static class HttpRegister
{
    public static WebApplicationBuilder AddHttpService(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        return builder;
    }
}