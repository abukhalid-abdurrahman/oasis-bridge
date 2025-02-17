namespace Infrastructure.ImplementationContract;

public sealed class NetworkTokenService : INetworkTokenService
{
    public async Task<Result<PagedResponse<IEnumerable<GetNetworkTokenResponse>>>> GetNetworkTokensAsync(
        NetworkTokenFilter filter, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<GetNetworkTokenDetailResponse>> GetNetworkTokenDetailAsync(Guid networkTokenId,
        CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<CreateNetworkTokenResponse>> CreateNetworkTokenAsync(CreateNetworkTokenRequest request,
        CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<UpdateNetworkTokenResponse>> UpdateNetworkTokenAsync(Guid networkTokenId,
        UpdateNetworkTokenRequest request, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<DeleteNetworkTokenResponse>> DeleteNetworkTokenAsync(Guid networkTokenId,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}