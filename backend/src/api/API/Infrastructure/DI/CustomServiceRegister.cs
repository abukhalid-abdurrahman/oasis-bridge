namespace API.Infrastructure.DI;

public static class CustomServiceRegister
{
    public static WebApplicationBuilder AddCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<Seeder>();
        //builder.Services.AddIpfs(builder.Configuration);
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<INetworkService, NetworkService>();
        builder.Services.AddScoped<IIdentityService, IdentityService>();
        builder.Services.AddScoped<IUserRoleService, UserRoleService>();
        builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
        builder.Services.AddScoped<INetworkTokenService, NetworkTokenService>();
        builder.Services.AddScoped<IWalletLinkedAccountService, WalletLinkedAccountService>();
        return builder;
    }
}