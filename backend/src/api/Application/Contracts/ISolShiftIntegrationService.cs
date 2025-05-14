using TransactionResponse = Application.DTOs.NftPurchase.Responses.TransactionResponse;

namespace Application.Contracts;

public interface ISolShiftIntegrationService
{
    Task<Result<TransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request);
    Task<Result<TransactionResponse>> SendTransactionAsync(string transactionId);
}