namespace API.Infrastructure.DI;

public static class RegisterServices
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddSwaggerGen();


        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IRoleService, IRoleService>();
        builder.Services.AddScoped<IUserRoleService, UserRoleService>();


        return builder;
    }
}