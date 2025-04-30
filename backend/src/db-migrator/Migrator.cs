namespace db_migrator;

public class Migrator
{
    public static async Task MigrateAsync(IHost host, CancellationToken cancellationToken = default)
    {
        ILogger logger = host.Services.GetRequiredService<ILogger<Migrator>>();
        DateTimeOffset date = DateTimeOffset.UtcNow;
        logger.OperationStarted(nameof(MigrateAsync), date);

        try
        {
            using IServiceScope scope = host.Services.CreateScope();
            DataContext db = scope.ServiceProvider.GetRequiredService<DataContext>();
            await db.Database.MigrateAsync(cancellationToken);
            logger.OperationCompleted(nameof(MigrateAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
        }
        catch (Exception ex)
        {
            logger.OperationException(nameof(MigrateAsync), ex.ToString());
            logger.OperationCompleted(nameof(MigrateAsync), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
        }
    }
}