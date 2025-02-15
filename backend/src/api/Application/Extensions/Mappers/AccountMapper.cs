namespace Application.Extensions.Mappers;

public static class AccountMapper
{
    public static User ToEntity(this RegisterRequest request, IHttpContextAccessor accessor)
    {
        return new()
        {
            CreatedBy = accessor.GetId(),
            Email = request.EmailAddress,
            PhoneNumber = request.PhoneNumber,
            UserName = request.UserName,
            PasswordHash = HashingUtility.ComputeSha256Hash(request.Password),
            TokenVersion = Guid.NewGuid(),
            CreatedByIp = accessor.GetRemoteIpAddress()
        };
    }
}