using System.Diagnostics;
using System.IO;

namespace Guardian.App;

public sealed class DaemonSupervisor
{
    private const string DaemonProcessName = "Guardian.Daemon";
    private const string DaemonExecutableName = "Guardian.Daemon.exe";

    public bool IsDaemonRunning() =>
        Process.GetProcessesByName(DaemonProcessName).Any(p =>
        {
            try
            {
                return !p.HasExited;
            }
            catch
            {
                return false;
            }
            finally
            {
                p.Dispose();
            }
        });

    public bool TryEnsureDaemonRunning(out string message)
    {
        if (IsDaemonRunning())
        {
            message = "守护进程正在运行";
            return true;
        }

        var daemonPath = FindDaemonExecutable();
        if (daemonPath is null)
        {
            message = "未找到 Guardian.Daemon.exe，守护进程暂未启动";
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = daemonPath,
                WorkingDirectory = Path.GetDirectoryName(daemonPath) ?? AppContext.BaseDirectory,
                UseShellExecute = false
            });
            message = "已尝试启动守护进程";
            return true;
        }
        catch (Exception ex)
        {
            message = $"启动守护进程失败：{ex.Message}";
            return false;
        }
    }

    private static string? FindDaemonExecutable()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDirectory, DaemonExecutableName),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "Daemon", DaemonExecutableName)),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "Guardian.Daemon", "bin", "Debug", "net8.0", DaemonExecutableName)),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "Guardian.Daemon", "bin", "Release", "net8.0", DaemonExecutableName)),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "Guardian.Daemon", "bin", "Debug", "net8.0-windows", DaemonExecutableName)),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "Guardian.Daemon", DaemonExecutableName))
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
