namespace Application.Extensions.Mappers;

public static class AccountMapper
{
    public static User ToEntity(this RegisterRequest request, Guid createdBy)
    {
        return new()
        {
            CreatedBy = createdBy,
            Email = request.EmailAddress,
            PhoneNumber = request.PhoneNumber,
            UserName = request.UserName,
            PasswordHash = HashingUtility.ComputeSha256Hash(request.Password)
        };
    }
}