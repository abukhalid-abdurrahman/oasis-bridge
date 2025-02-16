namespace API.Infrastructure.DI;

public static class CustomServiceRegister
{
    public static WebApplicationBuilder AddCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IRoleService, IRoleService>();
        builder.Services.AddScoped<IUserRoleService, UserRoleService>();
        return builder;
    }
}