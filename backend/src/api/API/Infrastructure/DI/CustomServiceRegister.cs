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
        // Registering the Seeder service for initializing or seeding data in the application.
        builder.Services.AddScoped<Seeder>();

        // Registering role-related services.
        builder.Services.AddScoped<IRoleService, RoleService>();

        // Registering user-related services.
        builder.Services.AddScoped<IUserService, UserService>();

        // Registering order-related services.
        builder.Services.AddScoped<IOrderService, OrderService>();

        // Register ipfs-services
        builder.Services.AddScoped<IIpfsService, IpfsService>();

        // Registering network-related services.
        builder.Services.AddScoped<INetworkService, NetworkService>();

        // Registering identity-related services (authentication, registration, etc.).
        builder.Services.AddScoped<IIdentityService, IdentityService>();

        // Registering user-role management services.
        builder.Services.AddScoped<IUserRoleService, UserRoleService>();

        // Registering exchange rate services, which could be used for financial calculations.
        builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();

        // Registering network token-related services, which may involve operations with tokens on a network.
        builder.Services.AddScoped<INetworkTokenService, NetworkTokenService>();

        // Registering wallet and linked account services for managing wallets and associated accounts.
        builder.Services.AddScoped<IWalletLinkedAccountService, WalletLinkedAccountService>();

        // Returning the builder for chaining.
        return builder;
    }
}