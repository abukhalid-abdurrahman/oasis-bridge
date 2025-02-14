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

    public static UserRole ToEntity(this CreateUserRoleRequest request, Guid createdById)
        => new()
        {
            UserId = request.UserId,
            RoleId = request.RoleId,
            CreatedBy = createdById
        };

    public static UserRole ToEntity(this UserRole userRole, Guid deletedById)
    {
        userRole.Delete(deletedById);
        return userRole;
    }

    public static UserRole ToEntity(this UserRole userRole, Guid updatedById, UpdateUserRoleRequest request)
    {
        userRole.Update(updatedById);
        userRole.UserId = request.UserId;
        userRole.RoleId = request.RoleId;
        return userRole;
    }
}