namespace API.Infrastructure.Middlewares;

public static class RegisterMiddlewares
{
    public static WebApplication MapMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<RequestTimingMiddleware>();

        app.UseMiddleware<LoggingMiddleware>();

        app.UseHttpLogging();

        app.UseHttpsRedirection();

        app.UseExceptionHandler("/error");

        app.UseResponseCompression();

        app.UseRateLimiter();

        app.UseMiddleware<RequestCancellationMiddleware>();

        app.UseCors("AllowAll");

        app.UseAuthentication();

        app.UseMiddleware<TokenValidationMiddleware>();

        app.UseAuthorization();


        app.UseRouting();

        app.MapControllers();

        app.Run();

        return app;
    }
}