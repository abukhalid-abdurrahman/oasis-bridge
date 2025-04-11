namespace Infrastructure.DataAccess.EntityConfigurations;

public sealed class NetworkConfig : IEntityTypeConfiguration<Network>
{
    public void Configure(EntityTypeBuilder<Network> builder)
    {
        builder.HasIndex(r => r.Name).IsUnique();

    }
}