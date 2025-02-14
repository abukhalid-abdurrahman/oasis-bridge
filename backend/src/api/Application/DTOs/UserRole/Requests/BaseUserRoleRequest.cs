namespace Application.DTOs.UserRole.Requests;

public abstract record BaseUserRoleRequest( 
    Guid UserId,
    Guid RoleId);