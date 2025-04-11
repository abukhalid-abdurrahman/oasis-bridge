namespace Infrastructure.DataAccess.EntityConfigurations;

public sealed class NetworkTokenConfig : IEntityTypeConfiguration<NetworkToken>
{
    public void Configure(EntityTypeBuilder<NetworkToken> builder)
    {
        builder.HasIndex(r => r.Symbol).IsUnique();

    }
}