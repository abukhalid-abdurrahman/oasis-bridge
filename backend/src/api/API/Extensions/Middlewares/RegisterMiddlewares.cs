namespace API.Extensions.Middlewares;

public static class RegisterMiddlewares
{
    public static WebApplication MapMiddlewares(this WebApplication app)
    {
        app.UseCors("AllowAll");
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseExceptionHandler("/error");
        app.MapControllers();
        app.Run();

        return app;
    }
}