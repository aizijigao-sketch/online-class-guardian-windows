using System.Diagnostics;
using System.IO;

namespace Guardian.App;

public sealed class DaemonSupervisor
{
    private const string DaemonProcessName = "Guardian.Daemon";
    private const string DaemonExecutableName = "Guardian.Daemon.exe";
    private const string TaskName = "OnlineClassGuardian";

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
            message = "正在运行";
            return true;
        }

        if (TryRunScheduledTask(out var taskMessage))
        {
            message = taskMessage;
            return true;
        }

        var daemonPath = FindDaemonExecutable();
        if (daemonPath is null)
        {
            message = "未找到 Guardian.Daemon.exe，请重新发布程序。";
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = daemonPath,
                WorkingDirectory = Path.GetDirectoryName(daemonPath) ?? AppContext.BaseDirectory,
                UseShellExecute = true,
                Verb = "runas"
            });
            message = "已请求管理员权限启动守护进程。";
            return true;
        }
        catch (Exception ex)
        {
            message = $"启动失败：{ex.Message}。请用家长工具重新安装开机启动任务。";
            return false;
        }
    }

    private static bool TryRunScheduledTask(out string message)
    {
        try
        {
            using var query = Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = $"/Query /TN {TaskName}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            query?.WaitForExit(3000);
            if (query is null || query.ExitCode != 0)
            {
                message = "未安装管理员启动任务。";
                return false;
            }

            using var run = Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = $"/Run /TN {TaskName}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            run?.WaitForExit(3000);
            if (run is not null && run.ExitCode == 0)
            {
                message = "已通过管理员计划任务启动。";
                return true;
            }

            message = "计划任务启动失败，请重新安装开机启动任务。";
            return false;
        }
        catch (Exception ex)
        {
            message = $"计划任务检查失败：{ex.Message}";
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
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "Guardian.Daemon", DaemonExecutableName))
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
