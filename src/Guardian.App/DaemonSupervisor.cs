using System.Diagnostics;
using System.IO;
using Guardian.Shared;
using Guardian.Shared.Services;

namespace Guardian.App;

public sealed class DaemonSupervisor
{
    private const string DaemonProcessName = "Guardian.Daemon";
    private const string DaemonExecutableName = "Guardian.Daemon.exe";
    private readonly WindowsServiceManager _serviceManager = new();

    public bool IsDaemonRunning()
    {
        var service = _serviceManager.Query();
        if (service.IsRunning)
        {
            return true;
        }

        return Process.GetProcessesByName(DaemonProcessName).Any(p =>
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
    }

    public bool TryEnsureDaemonRunning(out string message)
    {
        var daemonPath = FindDaemonExecutable();
        if (daemonPath is null)
        {
            message = "未找到 Guardian.Daemon.exe，请重新发布程序。";
            return false;
        }

        var serviceStatus = _serviceManager.Query();
        if (serviceStatus.Exists &&
            serviceStatus.HasExpectedBinary(daemonPath) &&
            serviceStatus.HasExpectedConfig(AppPaths.ConfigPath))
        {
            if (serviceStatus.IsRunning)
            {
                message = "服务正在运行";
                return true;
            }

            var start = _serviceManager.Start();
            if (start.Success)
            {
                message = "服务已启动";
                return true;
            }
        }

        if (TryRunElevatedServiceCommand(daemonPath, "--install-service", "--start-service", "--config", AppPaths.ConfigPath))
        {
            message = serviceStatus.Exists ? "已请求管理员权限修复并启动服务" : "已请求管理员权限安装并启动服务";
            return true;
        }

        if (TryRunElevatedServiceCommand(daemonPath, "--config", AppPaths.ConfigPath))
        {
            message = "已请求管理员权限临时启动守护进程";
            return true;
        }

        message = "服务未就绪，请确认已允许管理员权限。";
        return false;
    }

    private static bool TryRunElevatedServiceCommand(string daemonPath, params string[] arguments)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = daemonPath,
                Arguments = string.Join(" ", arguments.Select(QuoteArgument)),
                WorkingDirectory = Path.GetDirectoryName(daemonPath) ?? AppContext.BaseDirectory,
                UseShellExecute = true,
                Verb = "runas"
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string QuoteArgument(string argument) =>
        argument.Contains(' ') || argument.Contains('"')
            ? $"\"{argument.Replace("\"", "\\\"")}\""
            : argument;

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
