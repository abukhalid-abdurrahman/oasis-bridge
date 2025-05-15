using Ipfs;
using Solnet.Wallet;
using Solnet.Wallet.Bip39;

namespace Infrastructure.ImplementationContract;

public sealed class NftPurchaseService(
    ILogger<NftPurchaseService> logger,
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ISolShiftIntegrationService solShiftService) : INftPurchaseService
{
    public async Task<Result<string>> CreateAsync(CreateNftPurchaseRequest request)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(CreateAsync), date);
        try
        {
            bool existingUser = await dbContext.Users.AnyAsync(x => x.Id == accessor.GetId());
            if (!existingUser)
                return Result<string>.Failure(ResultPatternError.NotFound(Messages.UserNotFound));

            RwaToken? existingRwa = await dbContext.RwaTokens
                .Include(x => x.VirtualAccount)
                .ThenInclude(x => x.Network)
                .FirstOrDefaultAsync(x => x.Id == request.RwaId);
            if (existingRwa is null)
                return Result<string>.Failure(ResultPatternError.NotFound(Messages.RwaTokenNotFound));

            WalletLinkedAccount? buyer = await dbContext.WalletLinkedAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PublicKey == request.BuyerPubKey);
            if (buyer is null)
                return Result<string>.Failure(
                    ResultPatternError.NotFound(Messages.CreateNftPurchaseBuyerAccountNotFound));

            string base58SecretKey = existingRwa.VirtualAccount.PrivateKey;

            if (existingRwa.VirtualAccount.Network.Name == Networks.Solana)
            {
                Mnemonic mnemonic = new(existingRwa.VirtualAccount.SeedPhrase);
                Wallet wallet = new(mnemonic);
                base58SecretKey = Base58.Encode(wallet.Account.PrivateKey);
            }


            Result<CreateTransactionResponse> resultOfTransaction =
                await solShiftService.CreateTransactionAsync(new(
                    buyer.PublicKey,
                    existingRwa.VirtualAccount.PublicKey,
                    base58SecretKey,
                    existingRwa.MintAccount,
                    existingRwa.Price,
                    null));
            if (!resultOfTransaction.IsSuccess)
                return Result<string>.Failure(resultOfTransaction.Error);


            string transactionHash = resultOfTransaction.Value.Data.Transaction;
            RwaTokenOwnershipTransfer newObj = new()
            {
                Price = existingRwa.Price,
                CreatedBy = accessor.GetId(),
                CreatedByIp = accessor.GetRemoteIpAddress(),
                BuyerWalletId = buyer.Id,
                SellerWalletId = existingRwa.VirtualAccountId,
                TransactionDate = DateTimeOffset.UtcNow,
                TransactionHash = transactionHash,
                RwaTokenId = existingRwa.Id,
                TransferStatus = RwaTokenOwnershipTransferStatus.InProgress
            };

            await dbContext.RwaTokenOwnershipTransfers.AddAsync(newObj);
            return await dbContext.SaveChangesAsync() != 0
                ? Result<string>.Success(transactionHash)
                : Result<string>.Failure(ResultPatternError.InternalServerError(Messages.CreateNftPurchaseFailed));
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(CreateAsync), ex.Message);
            logger.OperationCompleted(nameof(CreateAsync), DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow - date);
            return Result<string>.Failure(
                ResultPatternError.InternalServerError(ex.Message));
        }
    }

    public async Task<Result<string>> SendAsync(string transactionHash)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(SendAsync), date);
        try
        {
            RwaTokenOwnershipTransfer? existingRwaTokenOwner = await dbContext.RwaTokenOwnershipTransfers
                .FirstOrDefaultAsync(x => x.TransactionHash == transactionHash);
            if (existingRwaTokenOwner is null)
                return Result<string>.Failure(
                    ResultPatternError.NotFound(Messages.RwaTokenOwnershipTransferNotFound));

            RwaToken? existingRwaToken =
                await dbContext.RwaTokens
                    .FirstOrDefaultAsync(x => x.Id == existingRwaTokenOwner.RwaTokenId);
            if (existingRwaToken is null)
                return Result<string>.Failure(ResultPatternError.NotFound(Messages.RwaTokenNotFound));

            Result<SendTransactionResponse> resultOfSendTransaction =
                await solShiftService.SendTransactionAsync(new(transactionHash));
            if (!resultOfSendTransaction.IsSuccess)
                return Result<string>.Failure(resultOfSendTransaction.Error);

            existingRwaToken.VirtualAccountId = existingRwaTokenOwner.BuyerWalletId;
            existingRwaTokenOwner.TransactionHash = resultOfSendTransaction.Value.Data.TransactionId;
            existingRwaTokenOwner.TransferStatus = RwaTokenOwnershipTransferStatus.Completed;

            return await dbContext.SaveChangesAsync() != 0
                ? Result<string>.Success(resultOfSendTransaction.Value.Data.TransactionId)
                : Result<string>.Failure(ResultPatternError.InternalServerError(Messages.SendNftPurchaseFailed));
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(SendAsync), ex.Message);
            logger.OperationCompleted(nameof(SendAsync), DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow - date);
            return Result<string>.Failure(
                ResultPatternError.InternalServerError(ex.Message));
        }
    }
}