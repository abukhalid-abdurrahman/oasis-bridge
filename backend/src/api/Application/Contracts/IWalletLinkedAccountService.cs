namespace Application.Contracts;

public interface IWalletLinkedAccountService
{
    Task<BaseResult> CreateAsync(CreateWalletLinkedAccountRequest request,CancellationToken token = default!);

    Task<Result<IEnumerable<GetWalletLinkedAccountDetailResponse>>>
        GetAsync(CancellationToken token = default!);
}