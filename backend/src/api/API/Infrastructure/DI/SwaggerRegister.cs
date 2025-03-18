namespace API.Infrastructure.DI;

public static class SwaggerRegister
{
    public static WebApplicationBuilder AddSwaggerService(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.Name);
            options.ResolveConflictingActions(apiDescriptions =>
            {
                var first = apiDescriptions.First();
                Console.WriteLine($"⚠️ Conflict in route Swagger: {first.RelativePath}");
                return first;
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Scheme = "Bearer",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer 1234aaabbbccc\""
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Name = "Authorization",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            options.EnableAnnotations();
        });

        return builder;
    }
}