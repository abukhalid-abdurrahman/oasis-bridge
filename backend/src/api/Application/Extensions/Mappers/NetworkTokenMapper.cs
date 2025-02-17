namespace Application.Extensions.Mappers;

public static class NetworkTokenMapper
{
    public static GetNetworkTokenResponse ToRead(this NetworkToken token)
        => new(
            token.Id,
            token.Symbol,
            token.Description,
            token.NetworkId,
            token.Network.ToReadDetail());

    public static GetNetworkTokenDetailResponse ToReadDetail(this NetworkToken token)
        => new(
            token.Id,
            token.Symbol,
            token.Description,
            token.NetworkId,
            token.Network.ToReadDetail());

    public static NetworkToken ToEntity(this NetworkToken token, IHttpContextAccessor accessor,
        UpdateNetworkTokenRequest request)
    {
        token.Update(accessor.GetId());
        token.Symbol = request.Symbol;
        token.Description = request.Description;
        token.NetworkId = request.NetworkId;
        token.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        return token;
    }

    public static NetworkToken ToEntity(this CreateNetworkTokenRequest request, IHttpContextAccessor accessor)
        => new()
        {
            Symbol = request.Symbol,
            Description = request.Description,
            NetworkId = request.NetworkId,
            CreatedBy = accessor.GetId(),
            CreatedByIp = accessor.GetRemoteIpAddress()
        };

    public static NetworkToken ToEntity(this NetworkToken token, IHttpContextAccessor accessor)
    {
        token.Delete(accessor.GetId());
        token.DeletedByIp = accessor.GetRemoteIpAddress();
        return token;
    }
}