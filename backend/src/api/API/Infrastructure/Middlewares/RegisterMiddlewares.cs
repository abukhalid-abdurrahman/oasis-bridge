namespace API.Infrastructure.Middlewares;

public static class RegisterMiddlewares
{
    public static async Task<WebApplication> MapMiddlewares(this WebApplication app)
    {
        {
            using IServiceScope scope = app.Services.CreateScope();
            Seeder seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
            await seeder.InitialAsync();
        }

        app.UseMiddleware<RequestTimingMiddleware>();

        app.UseMiddleware<LoggingMiddleware>();

        app.UseHttpLogging();

        app.UseHttpsRedirection();

        app.UseExceptionHandler("/error");

        app.UseResponseCompression();

        app.UseRateLimiter();

        app.UseMiddleware<RequestCancellationMiddleware>();

        app.UseCors("AllowAll");

        app.UseSwagger();
        
        app.UseSwaggerUI();
        
        app.UseAuthentication();

        app.UseMiddleware<TokenValidationMiddleware>();

        app.UseAuthorization();
        
        app.MapControllers();

        await app.RunAsync();

        return app;
    }
}