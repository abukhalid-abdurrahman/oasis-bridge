namespace Infrastructure.Extensions;

/// <summary>
/// Extension methods for generating JWT tokens in the context of user authentication.
/// </summary>
public static class Jwt
{
    /// <summary>
    /// Generates a JWT token for a given user asynchronously.
    /// </summary>
    /// <param name="dbContext">The database context to retrieve user roles from.</param>
    /// <param name="user">The user for whom the token will be generated.</param>
    /// <param name="config">The application configuration containing JWT settings (key, issuer, audience).</param>
    /// <returns>Result containing the generated JWT token and its expiration information.</returns>
    /// <exception cref="InvalidOperationException">Thrown if JWT settings (key, issuer, or audience) are missing in the configuration.</exception>
    public static async Task<Result<LoginResponse>> GenerateTokenAsync(
        this DataContext dbContext,
        User user,
        IConfiguration config)
    {
        // Retrieve JWT configuration values from the settings
        string? key = config["Jwt:key"];
        string? issuer = config["Jwt:issuer"];
        string? audience = config["Jwt:audience"];

        // Validate that JWT configuration is complete
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            throw new InvalidOperationException("JWT key, issuer, or audience is missing in configuration.");

        // Define the signing credentials using the symmetric key and HMAC SHA-256 algorithm
        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        // Create a list of claims associated with the user
        List<Claim> claims =
        [
            new(CustomClaimTypes.Id, user.Id.ToString()),
            new(CustomClaimTypes.UserName, user.UserName),
            new(CustomClaimTypes.Email, user.Email),
            new(CustomClaimTypes.Phone, user.PhoneNumber),
            new(CustomClaimTypes.FirstName, user.FirstName ?? ""),
            new(CustomClaimTypes.LastName, user.LastName ?? ""),
            new(CustomClaimTypes.TokenVersion, user.TokenVersion.ToString()),
        ];

        // Add user roles as claims
        claims.AddRange(await dbContext.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => new Claim(CustomClaimTypes.Role, ur.Role.Name))
            .AsNoTracking()
            .ToListAsync());

        // Define the expiration time for the token (30 minutes from the current UTC time)
        DateTime current = DateTime.UtcNow;

        // Create the JWT token
        JwtSecurityToken jwt = new(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: current.AddMinutes(30),
            signingCredentials: credentials);

        // Return the result with the token and expiration info
        return Result<LoginResponse>.Success(new(
            new JwtSecurityTokenHandler().WriteToken(jwt),
            current,
            current.AddMinutes(30)));
    }
}
