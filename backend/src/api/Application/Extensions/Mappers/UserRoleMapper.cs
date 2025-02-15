namespace Application.Extensions.Mappers;

public static class UserRoleMapper
{
    public static GetUserRolesResponse ToRead(this UserRole userRole)
        => new(
            userRole.UserId,
            userRole.RoleId,
            userRole.User.ToReadPublicDetail(),
            userRole.Role.ToReadDetail());

    public static GetUserRoleDetailResponse ToReadDetail(this UserRole userRole)
        => new(
            userRole.UserId,
            userRole.RoleId,
            userRole.User.ToReadPublicDetail(),
            userRole.Role.ToReadDetail());

    public static UserRole ToEntity(this CreateUserRoleRequest request, IHttpContextAccessor accessor)
        => new()
        {
            UserId = request.UserId,
            RoleId = request.RoleId,
            CreatedBy = accessor.GetId(),
            CreatedByIp = accessor.GetRemoteIpAddress()
        };

    public static UserRole ToEntity(this UserRole userRole, IHttpContextAccessor accessor)
    {
        userRole.Delete(accessor.GetId());
        userRole.DeletedByIp = accessor.GetRemoteIpAddress();
        return userRole;
    }

    public static UserRole ToEntity(this UserRole userRole, IHttpContextAccessor accessor,
        UpdateUserRoleRequest request)
    {
        userRole.Update(accessor.GetId());
        userRole.UserId = request.UserId;
        userRole.RoleId = request.RoleId;
        userRole.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        return userRole;
    }
}