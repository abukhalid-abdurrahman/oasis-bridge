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

    public static Role ToEntity(this Role role, IHttpContextAccessor accessor, UpdateRoleRequest request)
    {
        role.Update(accessor.GetId());
        role.RoleKey = request.RoleKey;
        role.Description = request.Description;
        role.Name = request.RoleName;
        role.UpdatedByIp!.Add(accessor.GetRemoteIpAddress());
        return role;
    }

    public static Role ToEntity(this CreateRoleRequest request, IHttpContextAccessor accessor)
        => new()
        {
            Name = request.RoleName,
            RoleKey = request.RoleKey,
            Description = request.Description,
            CreatedBy = accessor.GetId(),
            CreatedByIp = accessor.GetRemoteIpAddress()
        };

    public static Role ToEntity(this Role role, IHttpContextAccessor accessor)
    {
        role.Delete(accessor.GetId());
        role.DeletedByIp = accessor.GetRemoteIpAddress();
        return role;
    }
}