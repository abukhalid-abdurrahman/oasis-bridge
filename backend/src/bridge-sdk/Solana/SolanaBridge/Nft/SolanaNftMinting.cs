using System.Text;
using Common.Constants;
using Common.Contracts.Nft;
using Solnet.Metaplex.NFT;
using Solnet.Metaplex.NFT.Library;

namespace SolanaBridge.Nft;

public interface ISolanaNftMinting : INftMinting;

public sealed class SolanaNftMinting(
    INftWalletProvider walletProvider,
    INftMetadataSerializer metadataSerializer,
    MetadataClient metaplexClient,
    IRpcClient client) : ISolanaNftMinting
{
    /// <inheritdoc />
    public async Task<Result<NftMintingResponse>> MintAsync(Common.DTOs.Nft nft, CancellationToken token = default)
    {
        Result<WalletKeyPair> walletResult = await walletProvider.GetWalletAsync(Networks.Solana, token);
        if (!walletResult.IsSuccess)
            return Result<NftMintingResponse>.Failure(walletResult.Error);

        WalletKeyPair walletKey = walletResult.Value!;
        Mnemonic mnemonic = new(Encoding.UTF8.GetString(walletKey.SeedPhrease));
        Wallet wallet = new(mnemonic);

        Account account = wallet.Account;
        Account mintAccount = new();

        Result<string> metadataResult = await metadataSerializer.SerializeAsync(nft, token);
        if (!metadataResult.IsSuccess)
            return Result<NftMintingResponse>.Failure(metadataResult.Error);

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
            sellerFeeBasisPoints = ushort.TryParse(nft.Royality, out ushort fee) ? fee : (ushort)500,
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
                return Result<NftMintingResponse>.Success(
                    new(
                        mintAccount.PublicKey,
                        tx.Result,
                        uri,
                        Networks.Solana));
            }

            if (tx.ErrorData.Error.Type == TransactionErrorType.AccountNotFound)
                return Result<NftMintingResponse>.Failure(
                    ResultPatternError.BadRequest(Messages.AccountNotFound));

            return Result<NftMintingResponse>.Failure(
                ResultPatternError.InternalServerError(tx.Reason));
        }
        catch (Exception ex)
        {
            return Result<NftMintingResponse>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<Common.DTOs.Nft>> GetMetadataAsync(string nftAddress, CancellationToken token = default)
    {
        try
        {
            MetadataAccount account = await MetadataAccount.GetAccount(client, new PublicKey(nftAddress));

            Dictionary<string, string>? attributes = account.offchainData.attributes?
                .Where(attr => attr != null)
                .ToDictionary(x => x.trait_type, x => x.value);

            return Result<Common.DTOs.Nft>.Success(new Common.DTOs.Nft
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
            return Result<Common.DTOs.Nft>.Failure(ResultPatternError.InternalServerError(ex.Message));
        }
    }
}