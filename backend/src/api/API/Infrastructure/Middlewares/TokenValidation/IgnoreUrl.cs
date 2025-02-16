namespace API.Infrastructure.Middlewares.TokenValidation;

public static class IgnoreUrl
{
    public static readonly HashSet<string> IgnoreUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        $"/api/{ApiVersions.V1}/accounts/register",
        $"/api/{ApiVersions.V1}/accounts/login",
        $"/api/{ApiVersions.V1}/accounts/forgot-password",
        $"/api/{ApiVersions.V1}/accounts/reset-password",
        $"/api/{ApiVersions.V1}/accounts/restore",
        $"/api/{ApiVersions.V1}/accounts/restore/confirm",
    };
}