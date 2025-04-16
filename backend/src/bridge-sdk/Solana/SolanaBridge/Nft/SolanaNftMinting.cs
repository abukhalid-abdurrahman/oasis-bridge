using Solnet.Metaplex.NFT;
using Solnet.Metaplex.NFT.Library;

public sealed class SolanaNftMinting(
    INftWalletProvider walletProvider,
    INftMetadataSerializer metadataSerializer,
    MetadataClient metaplexClient,
    IRpcClient client
) : INFTMinting
{
    /// <inheritdoc />
    public async Task<Result<string>> MintAsync(Nft nft, CancellationToken token = default)
    {
        Result<WalletKeyPair> walletResult = await walletProvider.GetWalletAsync(token);
        if (!walletResult.IsSuccess)
            return Result<string>.Failure(walletResult.Error);

        WalletKeyPair wallet = walletResult.Value!;
        Account account = new(wallet.PrivateKey, wallet.PublicKey);
        Account mintAccount = new();

        Result<string> metadataResult = await metadataSerializer.SerializeAsync(nft, token);
        if (!metadataResult.IsSuccess)
            return Result<string>.Failure(metadataResult.Error);

        string uri = metadataResult.Value!;

        List<Creator> creators =
        [
            new(account.PublicKey, share: 100, verified: true)
        ];

        Metadata metadata = new()
        {
            name = nft.Name,
            symbol = nft.Symbol,
            uri = uri,
            sellerFeeBasisPoints = ushort.TryParse(nft.Royality, out ushort fee) ? (ushort)fee : (ushort)500,
            creators = creators
        };

        try
        {
            RequestResult<string> tx = await metaplexClient.CreateNFT(
                ownerAccount: account,
                mintAccount: mintAccount,
                tokenStandard: TokenStandard.NonFungible,
                metaData: metadata,
                isMasterEdition: true,
                isMutable: true
            );

            if (tx.WasSuccessful)
            {
                return Result<string>.Success(mintAccount.PublicKey);
            }

            return Result<string>.Failure(ResultPatternError.InternalServerError("Minting failed"));
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<Nft>> GetMetadataAsync(string nftAddress, CancellationToken token = default)
    {
        try
        {
            MetadataAccount account = await MetadataAccount.GetAccount(client, new PublicKey(nftAddress));

            Dictionary<string, string>? attributes = account.offchainData.attributes?
                .Where(attr => attr != null)
                .ToDictionary(x => x.trait_type, x => x.value);

            return Result<Nft>.Success(new Nft
            {
                Name = account.offchainData.name,
                Symbol = account.offchainData.symbol,
                Description = account.offchainData.description,
                ImageUrl = account.offchainData.default_image,
                Url = account.metadata.uri,
                Royality = account.metadata.sellerFeeBasisPoints.ToString(),
                AdditionalMetadata = attributes
            });
        }
        catch (Exception ex)
        {
            return Result<Nft>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }
}
