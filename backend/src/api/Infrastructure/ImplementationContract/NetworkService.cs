namespace Infrastructure.ImplementationContract;

public sealed class NetworkService : INetworkService
{
    public async Task<Result<PagedResponse<IEnumerable<GetNetworkResponse>>>> GetNetworksAsync(NetworkFilter filter,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<GetNetworkDetailResponse>> GetNetworkDetailAsync(Guid networkId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<CreateNetworkResponse>> CreateNetworkAsync(CreateNetworkRequest request,
        CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<UpdateNetworkResponse>> UpdateNetworkAsync(Guid networkId, UpdateNetworkRequest request,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<DeleteNetworkResponse>> DeleteNetworkAsync(Guid networkId,
        CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}