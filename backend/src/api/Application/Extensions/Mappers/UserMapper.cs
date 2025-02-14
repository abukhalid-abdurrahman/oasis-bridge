namespace Application.Extensions.Mappers;

public static class UserMapper
{
    public static GetAllUserResponse ToRead(this User user)
    {
        return new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.UserName,
            user.Dob);
    }

    public static GetUserDetailPrivateResponse ToReadPrivateDetail(this User user)
    {
        return new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.UserName,
            user.Dob,
            user.LastLoginAt,
            user.TotalLogins);
    }

    public static GetUserDetailPublicResponse ToReadPublicDetail(this User user)
    {
        return new(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.UserName,
            user.Dob);
    }

    public static User ToEntity(this User user, UpdateUserProfileRequest request, Guid updateById)
    {
        user.Update(updateById);
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.Dob = request.Dob;
        return user;
    }
}