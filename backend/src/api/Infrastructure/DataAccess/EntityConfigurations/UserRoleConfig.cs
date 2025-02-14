namespace Infrastructure.DataAccess.EntityConfigurations;

public sealed class UserRoleConfig : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasAlternateKey(x => new { x.UserId, x.RoleId });
    }
}