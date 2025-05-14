using TransactionResponse = Application.DTOs.NftPurchase.Responses.TransactionResponse;

namespace Infrastructure.ImplementationContract;

public sealed class NftPurchaseService(
    ILogger<NftPurchaseService> logger,
    DataContext dbContext,
    IHttpContextAccessor accessor,
    ISolShiftIntegrationService solShiftService) : INftPurchaseService
{
    public async Task<Result<Guid>> CreateAsync(Guid rwaId)
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(CreateAsync), date);
        try
        {
            bool existingUser = await dbContext.Users.AnyAsync(x => x.Id == accessor.GetId());
            if (existingUser)
                return Result<Guid>.Failure(ResultPatternError.NotFound(Messages.UserNotFound));

            RwaToken? existingRwa = await dbContext.RwaTokens
                .Include(x => x.VirtualAccount)
                .FirstOrDefaultAsync(x => x.Id == rwaId);
            if (existingRwa is null)
                return Result<Guid>.Failure(ResultPatternError.NotFound(Messages.RwaTokenNotFound));

            VirtualAccount? buyer = await dbContext.VirtualAccounts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == accessor.GetId() &&
                                          x.NetworkId == existingRwa.VirtualAccount.NetworkId);

            if (buyer is null)
                return Result<Guid>.Failure(
                    ResultPatternError.NotFound(Messages.CreateNftPurchaseBuyerAccountNotFound));

            Result<TransactionResponse> resultOfTransaction =
                await solShiftService.CreateTransactionAsync(new(
                    buyer.PublicKey,
                    existingRwa.VirtualAccount.PublicKey,
                    existingRwa.VirtualAccount.PrivateKey,
                    existingRwa.MintAccount,
                    existingRwa.Price,
                    null));
            if (!resultOfTransaction.IsSuccess)
                return Result<Guid>.Failure(resultOfTransaction.Error);

            Result<TransactionResponse> resultOfSendTra =
                await solShiftService.SendTransactionAsync(resultOfTransaction.Value.Data.TransactionId);
            if (!resultOfSendTra.IsSuccess)
                return Result<Guid>.Failure(resultOfSendTra.Error);

            RwaTokenOwnershipTransfer newObj = new()
            {
                Price = existingRwa.Price,
                CreatedBy = accessor.GetId(),
                CreatedByIp = accessor.GetRemoteIpAddress(),
                BuyerWalletId = buyer.Id,
                SellerWalletId = existingRwa.VirtualAccountId,
                TransactionDate = DateTimeOffset.UtcNow,
                TransactionHash = resultOfSendTra.Value.Data.TransactionId,
                RwaTokenId = existingRwa.Id
            };

            existingRwa.VirtualAccountId = buyer.Id;

            await dbContext.RwaTokenOwnershipTransfers.AddAsync(newObj);
            return await dbContext.SaveChangesAsync() != 0
                ? Result<Guid>.Success(newObj.Id)
                : Result<Guid>.Failure(ResultPatternError.InternalServerError(Messages.CreateNftPurchaseFailed));
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(CreateAsync), ex.Message);
            logger.OperationCompleted(nameof(CreateAsync), DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow - date);
            return Result<Guid>.Failure(
                ResultPatternError.InternalServerError(ex.Message));
        }
    }
}