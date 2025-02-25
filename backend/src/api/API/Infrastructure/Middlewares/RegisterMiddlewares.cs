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

        app.UseHttpLogging();
        app.UseHttpsRedirection();
        app.UseExceptionHandler("/error");
        app.UseResponseCompression();
        //app.UseRateLimiter();

        app.UseCors("AllowAll");

        app.UseRouting();
        app.UseAuthentication();
        app.UseMiddleware<TokenValidationMiddleware>();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapControllers();
        await app.RunAsync();

        return app;
    }
}