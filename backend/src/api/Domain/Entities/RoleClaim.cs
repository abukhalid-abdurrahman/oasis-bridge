namespace Domain.Entities;

public sealed class RoleClaim : BaseEntity
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;
}