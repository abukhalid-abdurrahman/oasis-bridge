namespace API.Infrastructure.DI;

public static class JwtRegister
{
    public static WebApplicationBuilder AddJwtService(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = Jwt.GetSymmetricSecurityKey(builder.Configuration["Jwt:key"]!),
                    ValidateIssuerSigningKey = true,
                };
            });

        return builder;
    }
}