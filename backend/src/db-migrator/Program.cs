const string pathConfig = "appsettings.json";
const string connectionStringName = "DefaultConnection";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile(pathConfig, optional: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<DataContext>(configure =>
        {
            string connectionString = context.Configuration.GetConnectionString(connectionStringName)
                                      ?? throw new InvalidOperationException();
            configure.UseNpgsql(connectionString);
        });
        services.AddLogging(logging => logging.AddConsole());
    })
    .Build();

await Migrator.MigrateAsync(host);
await host.StopAsync();
host.Dispose();