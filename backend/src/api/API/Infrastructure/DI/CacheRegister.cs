namespace API.Infrastructure.DI;

public static class CacheRegister
{
    public static WebApplicationBuilder AddCacheService(this WebApplicationBuilder builder)
    {
        const string cacheKey = "ipfsCacheSettings:ipfsCacheDurationSeconds";
        builder.Services.AddControllers(options =>
        {
            options.CacheProfiles.Add(CacheProfileNames.OptimizedNftLogoCache, new CacheProfile
            {
                Duration = builder.Configuration.GetRequiredInt(cacheKey),
                Location = ResponseCacheLocation.Any,
                NoStore = false
            });
        });
        return builder;
    }
}