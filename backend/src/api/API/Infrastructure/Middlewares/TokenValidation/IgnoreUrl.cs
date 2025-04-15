namespace API.Infrastructure.Middlewares.TokenValidation;

/// <summary>
/// A static class that holds a set of API URLs that should be excluded from token validation.
/// These URLs typically represent authentication-related endpoints that do not require an authenticated token for access.
/// </summary>
public static class IgnoreUrl
{
    /// <summary>
    /// A HashSet containing the API endpoints that should be ignored by the token validation middleware.
    /// This allows certain URLs (such as authentication endpoints) to be accessed without requiring an authenticated token.
    /// The URLs are stored in a case-insensitive manner to ensure proper comparison.
    /// </summary>
    public static readonly HashSet<string> IgnoreUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        // List of authentication-related API endpoints that do not require token validation
        $"/api/{ApiVersions.V1}/auth/register", // Registration endpoint
        $"/api/{ApiVersions.V1}/auth/login", // Login endpoint
        $"/api/{ApiVersions.V1}/auth/forgot-password", // Forgot password endpoint
        $"/api/{ApiVersions.V1}/auth/reset-password", // Reset password endpoint
        $"/api/{ApiVersions.V1}/auth/restore", // Restore account endpoint
        $"/api/{ApiVersions.V1}/auth/restore/confirm", // Confirm restore account endpoint
    };
}