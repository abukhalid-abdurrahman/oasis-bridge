namespace Application.Extensions.Mappers;

public static class RoleMapper
{
    public static GetRolesResponse ToRead(this Role role)
        => new(
            role.Id,
            role.Name,
            role.RoleKey,
            role.Description);

    public static GetRoleDetailResponse ToReadDetail(this Role role)
        => new(
            role.Id,
            role.Name,
            role.RoleKey,
            role.Description);

    public static Role ToEntity(this Role role, Guid updateById, UpdateRoleRequest request)
    {
        role.Update(updateById);
        role.RoleKey = request.RoleKey;
        role.Description = request.Description;
        role.Name = request.RoleName;
        return role;
    }

    public static Role ToEntity(this CreateRoleRequest request, Guid createdById)
        => new()
        {
            Name = request.RoleName,
            RoleKey = request.RoleKey,
            Description = request.Description,
            CreatedBy = createdById
        };

    public static Role ToEntity(this Role role, Guid deletedById)
    {
        role.Delete(deletedById);
        return role;
    }
}