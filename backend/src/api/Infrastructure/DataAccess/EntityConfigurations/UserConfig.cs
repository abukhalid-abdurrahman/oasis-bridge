namespace Infrastructure.DataAccess.EntityConfigurations;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(r => r.UserName).IsUnique();
        builder.HasIndex(r => r.Email).IsUnique();
        builder.HasIndex(r => r.PhoneNumber).IsUnique();
        builder.HasIndex(r => new { r.Email, r.PhoneNumber, r.UserName }).IsUnique();
    }
}