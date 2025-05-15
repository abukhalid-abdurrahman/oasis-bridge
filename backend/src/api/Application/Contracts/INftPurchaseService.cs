namespace Application.Contracts;

public interface INftPurchaseService
{
    Task<Result<string>> CreateAsync(CreateNftPurchaseRequest request);
    Task<Result<string>> SendAsync(string transactionHash);
}