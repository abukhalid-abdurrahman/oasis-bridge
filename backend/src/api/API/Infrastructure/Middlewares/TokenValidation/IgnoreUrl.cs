namespace API.Infrastructure.Middlewares.TokenValidation;

public static class IgnoreUrl
{
    public  static readonly HashSet<string> IgnoreUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth",
        "/api/health"
    };
}