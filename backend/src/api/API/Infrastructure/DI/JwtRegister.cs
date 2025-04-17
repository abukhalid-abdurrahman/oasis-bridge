namespace API.Infrastructure.DI;

/// <summary>
/// This class registers JWT authentication and authorization services within the application's DI container.
/// It configures the authentication middleware to validate and process JWT tokens in HTTP requests.
/// </summary>
public static class JwtRegister
{
    /// <summary>
    /// Registers JWT authentication and authorization services in the DI container.
    /// This method configures the JWT authentication scheme, including validation parameters such as
    /// issuer, audience, signing key, and token lifetime.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance to register services with.</param>
    /// <returns>The WebApplicationBuilder instance with JWT authentication services registered.</returns>
    public static WebApplicationBuilder AddJwtService(this WebApplicationBuilder builder)
    {
        // Retrieve JWT configuration settings from the application's configuration.
        string? jwtKey = builder.Configuration["Jwt:key"];
        string? issuer = builder.Configuration["Jwt:issuer"];
        string? audience = builder.Configuration["Jwt:audience"];

        // Validate the presence of necessary JWT configuration parameters.
        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            throw new InvalidOperationException("JWT key, issuer, or audience is missing in configuration.");

        // Configure JWT authentication middleware with the provided settings.
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Validate the issuer of the JWT token.
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    // Validate the audience of the JWT token.
                    ValidateAudience = true,
                    ValidAudience = audience,
                    // Ensure the token has not expired.
                    ValidateLifetime = true,
                    // Specify the key used to sign the JWT token.
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuerSigningKey = true,
                    // Remove time skew when validating the token's lifetime.
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Register authorization services, enabling role-based or policy-based access control.
        builder.Services.AddAuthorization();

        // Return the builder for method chaining.
        return builder;
    }
}