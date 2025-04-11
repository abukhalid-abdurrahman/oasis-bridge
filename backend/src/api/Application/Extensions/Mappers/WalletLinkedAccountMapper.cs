namespace Application.Extensions.Mappers;

public static class WalletLinkedAccountMapper
{
    public static WalletLinkedAccount ToEntity(this CreateWalletLinkedAccountRequest request,
        Guid networkId, IHttpContextAccessor accessor) => new()
    {
        CreatedBy = accessor.GetId(),
        CreatedByIp = accessor.GetRemoteIpAddress(),
        NetworkId = networkId,
        PublicKey = request.WalletAddress
    };

    public static GetWalletLinkedAccountDetailResponse ToRead(this WalletLinkedAccount entity)
        => new(
            entity.UserId,
            entity.PublicKey,
            entity.Network.Name,
            entity.LinkedAt
        );
}