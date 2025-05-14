namespace Application.Contracts;

public interface INftPurchaseService
{
    Task<Result<string>> CreateAsync(Guid rwaId);
    Task<Result<string>> SendAsync(string transactionHash);
}