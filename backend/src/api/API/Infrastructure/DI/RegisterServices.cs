using Application.Contracts;
using BuildingBlocks.Extensions.Smtp;
using Infrastructure.DataAccess.Seed;
using Infrastructure.ImplementationContract;

namespace API.Infrastructure.DI;

public static class RegisterServices
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<IBridge, RadixBridge.RadixBridge>();
        builder.Services.AddScoped<IRadixBridge, RadixBridge.RadixBridge>();
        builder.Services.AddScoped<IBridge, SolanaBridge.SolanaBridge>();
        builder.Services.AddScoped<ISolanaBridge, SolanaBridge.SolanaBridge>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IRoleService, IRoleService>();
        builder.Services.AddScoped<IUserRoleService, UserRoleService>();

        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();


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
            NetworkId = (byte)(builder.Configuration["RadixTechnicalAccountBridgeOptions:NetworkId"] == RadixBridgeHelper.MainNet
                ? 0x01
                : 0x02),
        });


        return builder;
    }
}