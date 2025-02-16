namespace API.Infrastructure.DI;

public static class ResponseCompressionRegister
{
    public static WebApplicationBuilder AddResponseCompressionService(this WebApplicationBuilder builder)
    {
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        return builder;
    }
}