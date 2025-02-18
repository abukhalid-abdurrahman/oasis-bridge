namespace API.Infrastructure.DI;

public static class CustomServiceRegister
{
    public static WebApplicationBuilder AddCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<Seeder>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<INetworkService, NetworkService>();
        builder.Services.AddScoped<IIdentityService, IdentityService>();
        builder.Services.AddScoped<IUserRoleService, UserRoleService>();
        builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
        builder.Services.AddScoped<INetworkTokenService, NetworkTokenService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        
        return builder;
    }
}