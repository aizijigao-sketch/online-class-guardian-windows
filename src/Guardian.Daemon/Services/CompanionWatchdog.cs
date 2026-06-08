using System.Diagnostics;

namespace Guardian.Daemon.Services;

public sealed class CompanionWatchdog
{
    private const string AppProcessName = "Guardian.App";
    private const string AppExecutableName = "Guardian.App.exe";
    private DateTimeOffset _lastStartAttempt = DateTimeOffset.MinValue;

    public void EnsureAppRunning()
    {
        if (IsAppRunning() || DateTimeOffset.Now - _lastStartAttempt < TimeSpan.FromSeconds(15))
        {
            return;
        }

        _lastStartAttempt = DateTimeOffset.Now;
        var appPath = FindAppExecutable();
        if (appPath is null)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = appPath,
                WorkingDirectory = Path.GetDirectoryName(appPath) ?? AppContext.BaseDirectory,
                UseShellExecute = false
            });
        }
        catch
        {
            // The monitor loop must keep running even if the UI cannot be restarted.
        }
    }

    private static bool IsAppRunning() =>
        Process.GetProcessesByName(AppProcessName).Any(static process =>
        {
            using (process)
            {
                try
                {
                    return !process.HasExited;
                }
                catch
                {
                    return false;
                }
            }
        });

    private static string? FindAppExecutable()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDirectory, AppExecutableName),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "Guardian.App", "bin", "Debug", "net8.0-windows", AppExecutableName)),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "App", AppExecutableName))
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
