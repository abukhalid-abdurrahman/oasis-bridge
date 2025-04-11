using Role = Domain.Entities.Role;

namespace Infrastructure.DataAccess;

public sealed class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
    public DbSet<UserClaim> UserClaims { get; set; }
    public DbSet<UserLogin> UserLogins { get; set; }
    public DbSet<UserVerificationCode> UserVerificationCodes { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RoleClaim> RoleClaims { get; set; }

    public DbSet<Network> Networks { get; set; }
    public DbSet<NetworkToken> NetworkTokens { get; set; }
    public DbSet<AccountBalance> AccountBalances { get; set; }
    public DbSet<VirtualAccount> VirtualAccounts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    public DbSet<WalletLinkedAccount> WalletLinkedAccounts { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Infrastructure).Assembly);
        modelBuilder.FilterSoftDeletedProperties();
    }
}