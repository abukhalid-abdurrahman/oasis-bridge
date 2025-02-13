namespace Domain.Entities;

public sealed class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;
}