using System.Diagnostics;
using System.Text.RegularExpressions;
using Guardian.Shared;

namespace Guardian.Shared.Services;

public sealed record WindowsServiceStatus(
    bool Exists,
    bool IsRunning,
    string? BinaryPath,
    string? State,
    string? ErrorMessage)
{
    public bool HasExpectedBinary(string expectedExecutablePath)
    {
        if (string.IsNullOrWhiteSpace(BinaryPath))
        {
            return false;
        }

        var actual = ExtractExecutablePath(BinaryPath);
        return string.Equals(
            Path.GetFullPath(actual),
            Path.GetFullPath(expectedExecutablePath),
            StringComparison.OrdinalIgnoreCase);
    }

    public bool HasExpectedConfig(string expectedConfigPath)
    {
        if (string.IsNullOrWhiteSpace(BinaryPath))
        {
            return false;
        }

        return BinaryPath.Contains(Path.GetFullPath(expectedConfigPath), StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractExecutablePath(string binaryPath)
    {
        var trimmed = binaryPath.Trim();
        if (trimmed.StartsWith('"'))
        {
            var end = trimmed.IndexOf('"', 1);
            if (end > 1)
            {
                return trimmed[1..end];
            }
        }

        var exeIndex = trimmed.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        return exeIndex >= 0 ? trimmed[..(exeIndex + 4)] : trimmed.Split(' ')[0];
    }
}

public sealed record WindowsServiceCommandResult(bool Success, string Message, WindowsServiceStatus? Status = null);

public sealed class WindowsServiceManager
{
    public WindowsServiceStatus Query() => Query(AppPaths.ServiceName);

    public WindowsServiceCommandResult InstallOrRepair(string executablePath, string configPath)
    {
        var fullPath = Path.GetFullPath(executablePath);
        var fullConfigPath = Path.GetFullPath(configPath);
        if (!File.Exists(fullPath))
        {
            return new WindowsServiceCommandResult(false, $"守护程序不存在：{fullPath}");
        }

        var status = Query();
        if (status.Exists && status.HasExpectedBinary(fullPath) && status.HasExpectedConfig(fullConfigPath))
        {
            return new WindowsServiceCommandResult(true, "服务已安装且路径正确。", status);
        }

        if (status.Exists)
        {
            Stop();
            var delete = RunSc(["delete", AppPaths.ServiceName]);
            if (!delete.Success)
            {
                return new WindowsServiceCommandResult(false, $"修复旧服务失败：{delete.Message}", status);
            }

            Thread.Sleep(1000);
        }

        var binaryPath = $"\"{fullPath}\" --service --config \"{fullConfigPath}\"";
        var create = RunSc([
            "create",
            AppPaths.ServiceName,
            "binPath=",
            binaryPath,
            "start=",
            "auto",
            "DisplayName=",
            AppPaths.ServiceDisplayName
        ]);

        if (!create.Success)
        {
            return new WindowsServiceCommandResult(false, $"创建服务失败：{create.Message}");
        }

        RunSc(["description", AppPaths.ServiceName, "开机自动启动的网课守护服务。"]);
        RunSc(["failure", AppPaths.ServiceName, "reset=", "86400", "actions=", "restart/60000/restart/60000/restart/300000"]);
        RunSc(["failureflag", AppPaths.ServiceName, "1"]);
        var created = Query();
        return new WindowsServiceCommandResult(created.Exists, created.Exists ? "服务已安装。" : "服务安装后未能查询到。", created);
    }

    public WindowsServiceCommandResult Start()
    {
        var status = Query();
        if (!status.Exists)
        {
            return new WindowsServiceCommandResult(false, "服务未安装。", status);
        }

        if (status.IsRunning)
        {
            return new WindowsServiceCommandResult(true, "服务正在运行。", status);
        }

        var start = RunSc(["start", AppPaths.ServiceName]);
        Thread.Sleep(1200);
        var current = Query();
        if (current.IsRunning)
        {
            return new WindowsServiceCommandResult(true, "服务已启动。", current);
        }

        return new WindowsServiceCommandResult(false, $"服务启动失败：{start.Message}", current);
    }

    public WindowsServiceCommandResult Stop()
    {
        var status = Query();
        if (!status.Exists)
        {
            return new WindowsServiceCommandResult(true, "服务未安装。", status);
        }

        if (!status.IsRunning)
        {
            return new WindowsServiceCommandResult(true, "服务未运行。", status);
        }

        var stop = RunSc(["stop", AppPaths.ServiceName]);
        Thread.Sleep(1200);
        var current = Query();
        return new WindowsServiceCommandResult(!current.IsRunning, current.IsRunning ? $"服务停止失败：{stop.Message}" : "服务已停止。", current);
    }

    public WindowsServiceCommandResult Remove()
    {
        Stop();
        var status = Query();
        if (!status.Exists)
        {
            return new WindowsServiceCommandResult(true, "服务未安装。", status);
        }

        var delete = RunSc(["delete", AppPaths.ServiceName]);
        return new WindowsServiceCommandResult(delete.Success, delete.Success ? "服务已删除。" : $"服务删除失败：{delete.Message}");
    }

    private static WindowsServiceStatus Query(string serviceName)
    {
        var query = RunSc(["query", serviceName]);
        if (!query.Success)
        {
            return new WindowsServiceStatus(false, false, null, null, query.Message);
        }

        var qc = RunSc(["qc", serviceName]);
        var state = ParseState(query.Output);
        var binaryPath = qc.Success ? ParseBinaryPath(qc.Output) : null;
        return new WindowsServiceStatus(true, string.Equals(state, "RUNNING", StringComparison.OrdinalIgnoreCase), binaryPath, state, null);
    }

    private static string? ParseState(string output)
    {
        var match = Regex.Match(output, @"STATE\s*:\s*\d+\s+(\S+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? ParseBinaryPath(string output)
    {
        var match = Regex.Match(output, @"BINARY_PATH_NAME\s*:\s*(.+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static ScResult RunSc(string[] arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "sc.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(10000);

            var message = string.IsNullOrWhiteSpace(error) ? output.Trim() : error.Trim();
            return new ScResult(process.ExitCode == 0, output, message);
        }
        catch (Exception ex)
        {
            return new ScResult(false, string.Empty, ex.Message);
        }
    }

    private sealed record ScResult(bool Success, string Output, string Message);
}
