using System.Diagnostics;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Extensions.Logger;

namespace db_migrator;

public static class Migrator
{
    private static readonly ILogger Logger = LoggerFactory.Create(_ => { })
        .CreateLogger(nameof(Migrator));

    public static void Migrate()
    {
        DateTimeOffset date = DateTimeOffset.UtcNow;
        Logger.OperationStarted(nameof(Migrate), date);

        const string commandMigrate = "ef database update -p ./Infrastructure/ -s ./API/";
        try
        {
            string currentDir = Directory.GetCurrentDirectory();
            string rootDir = Path.GetFullPath(Path.Combine(currentDir, ".."));
            string apiProjectDir = Path.Combine(rootDir, "api");

            ProcessStartInfo processInfo = new()
            {
                FileName = "dotnet",
                Arguments = commandMigrate,
                WorkingDirectory = apiProjectDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(processInfo);
            process!.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) Console.WriteLine(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) Console.Error.WriteLine("Error:" + e.Data);
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            Logger.OperationCompleted(nameof(Migrate), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
            if (process.ExitCode == 0)
               Logger.LogInformation("Database updated successfully.");
            else
                Logger.LogCritical($" Database update failed with code {process.ExitCode}");
        }
        catch (Exception ex)
        {
            Logger.OperationException(nameof(Migrate), ex.Message);
            Logger.OperationCompleted(nameof(Migrate), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow - date);
        }
    }
}