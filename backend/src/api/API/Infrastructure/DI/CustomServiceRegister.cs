using SolShiftIntegrationService = Infrastructure.ImplementationContract.SolShiftIntegrationService;

namespace API.Infrastructure.DI;

/// <summary>
/// This class provides an extension method to register custom services into the application's dependency injection (DI) container.
/// It includes a variety of services, such as user and role management, network services, exchange rate services, and others.
/// </summary>
public static class CustomServiceRegister
{
    /// <summary>
    /// Registers a set of custom services required by the application.
    /// The services registered include user management, role management, network-related services, identity services, 
    /// exchange rate services, and wallet account management services.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance used to register services.</param>
    /// <returns>The WebApplicationBuilder instance with the custom services registered, enabling method chaining.</returns>
    public static WebApplicationBuilder AddCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<Seeder>();

        builder.Services.AddScoped<IRoleService, RoleService>();

        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped<IOrderService, OrderService>();

        builder.Services.AddScoped<INetworkService, NetworkService>();

        builder.Services.AddScoped<IIdentityService, IdentityService>();

        builder.Services.AddScoped<IUserRoleService, UserRoleService>();

        builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

        builder.Services.AddScoped<ISolShiftIntegrationService, SolShiftIntegrationService>();

        builder.Services.AddScoped<IWalletLinkedAccountService, WalletLinkedAccountService>();

        builder.Services.AddScoped<IRwaTokenService, RwaTokenService>();

        builder.Services.AddScoped<IRwaTokenPriceHistoryService, RwaTokenPriceHistoryService>();

        return builder;
    }
}