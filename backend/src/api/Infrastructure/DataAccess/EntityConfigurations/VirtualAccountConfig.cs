namespace Infrastructure.DataAccess.EntityConfigurations;

public sealed class VirtualAccountConfig : IEntityTypeConfiguration<VirtualAccount>
{
    public void Configure(EntityTypeBuilder<VirtualAccount> builder)
    {
        builder.HasIndex(r => r.PublicKey).IsUnique();
        builder.HasIndex(r => r.PrivateKey).IsUnique();
        builder.HasIndex(r => r.SeedPhrase).IsUnique();
        builder.HasIndex(r => r.Address).IsUnique();

    }
}