namespace API.Infrastructure.DI;

public static class BridgeRegister
{
    public static WebApplicationBuilder AddBridgeService(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBridge, RadixBridge.RadixBridge>();
        builder.Services.AddScoped<IRadixBridge, RadixBridge.RadixBridge>();
        builder.Services.AddScoped<IBridge, SolanaBridge.SolanaBridge>();
        builder.Services.AddScoped<ISolanaBridge, SolanaBridge.SolanaBridge>();


        builder.Services.AddScoped<IRpcClient>(_ =>
            ClientFactory.GetClient(builder.Configuration["SolanaTechnicalAccountBridgeOptions:HostUri"] ?? ""));
        builder.Services.AddSingleton(new SolanaTechnicalAccountBridgeOptions
        {
            HostUri = builder.Configuration["SolanaTechnicalAccountBridgeOptions:HostUri"] ?? "",
            PrivateKey = builder.Configuration["SolanaTechnicalAccountBridgeOptions:PrivateKey"] ?? "",
            PublicKey = builder.Configuration["SolanaTechnicalAccountBridgeOptions:PublicKey"] ?? "",
        });

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