namespace Infrastructure.ImplementationContract;

public sealed class RwaTokenOwnershipTransferService(
    DataContext dbContext) : IRwaTokenOwnershipTransferService
{
    public async Task<Result<IEnumerable<GetRwaTokenOwnershipTransferResponse>>> GetAllAsync(
        CancellationToken token = default)
        => Result<IEnumerable<GetRwaTokenOwnershipTransferResponse>>.Success(await dbContext.RwaTokenOwnershipTransfers
            .AsNoTracking()
            .Include(x => x.BuyerWallet)
            .Include(x => x.SellerWallet)
            .Select(x => x.ToRead()).ToListAsync(token));
}