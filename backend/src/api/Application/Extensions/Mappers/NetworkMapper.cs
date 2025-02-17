namespace Application.Extensions.Mappers;

public static class NetworkMapper
{
    public static GetNetworkResponse ToRead(this Network network)
        => new(
            network.Id,
            network.Name,
            network.Description,
            network.NetworkType.ToString());

    public static GetNetworkDetailResponse ToReadDetail(this Network network)
        => new(
            network.Id,
            network.Name,
            network.Description,
            network.NetworkType.ToString());

    public static Network ToEntity(this Network network, IHttpContextAccessor accessor, UpdateNetworkRequest request)
    {
        network.Update(accessor.GetId());
        network.Name = request.Name;
        network.Description = request.Description;
        network.NetworkType = request.NetworkType;
        network.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        return network;
    }

    public static Network ToEntity(this CreateNetworkRequest request, IHttpContextAccessor accessor)
        => new()
        {
            Name = request.Name,
            Description = request.Description,
            NetworkType = request.NetworkType,
            CreatedBy = accessor.GetId(),
            CreatedByIp = accessor.GetRemoteIpAddress()
        };

    public static Network ToEntity(this Network network, IHttpContextAccessor accessor)
    {
        network.Delete(accessor.GetId());
        network.DeletedByIp = accessor.GetRemoteIpAddress();
        return network;
    }
}