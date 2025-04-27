using Common.Contracts.Nft;
using Infrastructure.ImplementationContract.Nft;
using SolanaBridge.Nft;
using Solnet.Metaplex.NFT;

namespace API.Infrastructure.DI;

public static class NftRegister
{
    public static WebApplicationBuilder AddNftService(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<INftWalletProvider, NftWalletProvider>();
        builder.Services.AddScoped<INftMetadataSerializer, NftMetadataSerializer>();
        builder.Services.AddScoped<INftMinting, SolanaNftMinting>();
        builder.Services.AddScoped<ISolanaNftMinting, SolanaNftMinting>();
        builder.Services.AddScoped<MetadataClient>();
        
        return builder;
    }
}