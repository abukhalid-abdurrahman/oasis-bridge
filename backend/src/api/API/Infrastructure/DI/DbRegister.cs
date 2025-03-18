namespace API.Infrastructure.DI;

public static class DbRegister
{
    public static WebApplicationBuilder AddDbService(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<DataContext>(configure =>
        {
            configure.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        return builder;
    }
}