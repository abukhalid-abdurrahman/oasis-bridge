namespace Application.Contracts;

public interface IRwaTokenOwnershipTransferService
{
    Task<Result<IEnumerable<GetRwaTokenOwnershipTransferResponse>>> GetAllAsync(CancellationToken token = default);
}