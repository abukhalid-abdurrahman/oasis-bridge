namespace API.Infrastructure.Middlewares.TokenValidation;

public static class IgnoreUrl
{
    public static readonly HashSet<string> IgnoreUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        $"/api/{ApiVersions.V1}/auth/register",
        $"/api/{ApiVersions.V1}/auth/login",
        $"/api/{ApiVersions.V1}/auth/forgot-password",
        $"/api/{ApiVersions.V1}/auth/reset-password",
        $"/api/{ApiVersions.V1}/auth/restore",
        $"/api/{ApiVersions.V1}/auth/restore/confirm",
    };
}