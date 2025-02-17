namespace Application.Extensions.Mappers;

public static class AccountMapper
{
    public static User ToEntity(this RegisterRequest request, IHttpContextAccessor accessor)
    {
        return new()
        {
            CreatedBy = HttpAccessor.SystemId,
            Email = request.EmailAddress,
            UserName = request.UserName,
            PasswordHash = HashingUtility.ComputeSha256Hash(request.Password),
            TokenVersion = Guid.NewGuid(),
            CreatedByIp = accessor.GetRemoteIpAddress()
        };
    }
}

