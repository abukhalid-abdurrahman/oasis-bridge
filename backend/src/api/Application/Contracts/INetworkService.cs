namespace Application.Contracts;

public interface INetworkService
{
    Task<Result<PagedResponse<IEnumerable<GetNetworkResponse>>>>
        GetNetworksAsync(NetworkFilter filter, CancellationToken token = default);

    Task<Result<GetNetworkDetailResponse>>
        GetNetworkDetailAsync(Guid networkId, CancellationToken token);

    Task<Result<CreateNetworkResponse>>
        CreateNetworkAsync(CreateNetworkRequest request, CancellationToken token);

    Task<Result<UpdateNetworkResponse>>
        UpdateNetworkAsync(Guid networkId, UpdateNetworkRequest request, CancellationToken token = default);

    Task<Result<DeleteNetworkResponse>> DeleteNetworkAsync(Guid networkId, CancellationToken token = default);
}