namespace Application.Contracts;

public interface INetworkTokenService
{
    Task<Result<PagedResponse<IEnumerable<GetNetworkTokenResponse>>>>
        GetNetworkTokensAsync(NetworkTokenFilter filter, CancellationToken token = default);

    Task<Result<GetNetworkTokenDetailResponse>>
        GetNetworkTokenDetailAsync(Guid networkTokenId, CancellationToken token);

    Task<Result<CreateNetworkTokenResponse>>
        CreateNetworkTokenAsync(CreateNetworkTokenRequest request, CancellationToken token);

    Task<Result<UpdateNetworkTokenResponse>>
        UpdateNetworkTokenAsync(Guid networkTokenId, UpdateNetworkTokenRequest request,
            CancellationToken token = default);

    Task<Result<DeleteNetworkTokenResponse>> DeleteNetworkTokenAsync(Guid networkTokenId,
        CancellationToken token = default);
}