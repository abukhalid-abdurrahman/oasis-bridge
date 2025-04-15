namespace API.Infrastructure.DI;

/// <summary>
/// Provides extension methods to register services related to different blockchain bridges
/// into the application's dependency injection container.
/// </summary>
public static class BridgeRegister
{
    /// <summary>
    /// Adds the bridge services for Radix and Solana into the DI container.
    /// Registers interfaces to their corresponding implementation classes, such as RadixBridge and SolanaBridge,
    /// as well as configuration options for connecting to the bridges.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance.</param>
    /// <returns>The WebApplicationBuilder instance with the bridge services registered.</returns>
    public static WebApplicationBuilder AddBridgeService(this WebApplicationBuilder builder)
    {
        // Registers the RadixBridge as an IBridge and IRadixBridge service
        builder.Services.AddScoped<IBridge, RadixBridge.RadixBridge>();
        builder.Services.AddScoped<IRadixBridge, RadixBridge.RadixBridge>();

        // Registers the SolanaBridge as an IBridge and ISolanaBridge service
        builder.Services.AddScoped<IBridge, SolanaBridge.SolanaBridge>();
        builder.Services.AddScoped<ISolanaBridge, SolanaBridge.SolanaBridge>();

        // Registers the Solana RPC client with configuration from app settings
        builder.Services.AddScoped<IRpcClient>(_ =>
            ClientFactory.GetClient(builder.Configuration["SolanaTechnicalAccountBridgeOptions:HostUri"] ?? ""));

        // Registers Solana-specific options as a singleton service with configuration values
        builder.Services.AddSingleton(new SolanaTechnicalAccountBridgeOptions
        {
            HostUri = builder.Configuration["SolanaTechnicalAccountBridgeOptions:HostUri"] ?? "",
            PrivateKey = builder.Configuration["SolanaTechnicalAccountBridgeOptions:PrivateKey"] ?? "",
            PublicKey = builder.Configuration["SolanaTechnicalAccountBridgeOptions:PublicKey"] ?? "",
        });

        // Registers Radix-specific options as a singleton service with configuration values
        builder.Services.AddSingleton(new RadixTechnicalAccountBridgeOptions()
        {
            HostUri = builder.Configuration["RadixTechnicalAccountBridgeOptions:HostUri"] ?? "",
            PrivateKey = builder.Configuration["RadixTechnicalAccountBridgeOptions:PrivateKey"] ?? "",
            PublicKey = builder.Configuration["RadixTechnicalAccountBridgeOptions:PublicKey"] ?? "",
            AccountAddress = builder.Configuration["RadixTechnicalAccountBridgeOptions:AccountAddress"] ?? "",
            NetworkId = (byte)(builder.Configuration["RadixTechnicalAccountBridgeOptions:NetworkId"] ==
                               RadixBridgeHelper.MainNet
                ? 0x01
                : 0x02),
        });

        return builder;
    }
}