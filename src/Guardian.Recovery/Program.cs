using System.ComponentModel;
using System.Diagnostics;
using Guardian.Shared;
using Guardian.Shared.Services;

const string DaemonProcessName = "Guardian.Daemon";
const string AppProcessName = "Guardian.App";

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("网课守护程序恢复工具");
Console.WriteLine("--------------------");

var exitCode = 0;

try
{
    DisableLockMode();
}
catch (Exception ex)
{
    exitCode = 1;
    Console.WriteLine($"[失败] 无法保存恢复配置：{ex.Message}");
}

RemoveWindowsService();
DeleteStartupTask();
StopProcessByName(DaemonProcessName);
StopProcessByName(AppProcessName);

Console.WriteLine("恢复流程完成。默认不会删除日志文件。");
Environment.ExitCode = exitCode;

static void DisableLockMode()
{
    var store = new ConfigStore();
    var config = store.Load();
    config.LockMode.Enabled = false;
    config.LockMode.LastEnabledAt = null;
    store.Save(config);

    Console.WriteLine($"[完成] 已关闭网课模式：{store.ConfigPath}");
}

static void RemoveWindowsService()
{
    var manager = new WindowsServiceManager();
    var result = manager.Remove();
    Console.WriteLine(result.Success
        ? $"[完成] {result.Message}"
        : $"[提示] {result.Message}");
}

static void StopProcessByName(string processName)
{
    var currentProcessId = Environment.ProcessId;
    Process[] processes;

    try
    {
        processes = Process.GetProcessesByName(processName);
    }
    catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
    {
        Console.WriteLine($"[跳过] 无法枚举 {processName}：{ex.Message}");
        return;
    }

    if (processes.Length == 0)
    {
        Console.WriteLine($"[完成] 未发现正在运行的 {processName}。");
        return;
    }

    foreach (var process in processes)
    {
        using (process)
        {
            if (process.Id == currentProcessId)
            {
                Console.WriteLine($"[跳过] 不结束当前恢复工具进程：PID {process.Id}");
                continue;
            }

            try
            {
                if (!process.HasExited && process.CloseMainWindow())
                {
                    process.WaitForExit(3000);
                }

                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit(3000);
                }

                Console.WriteLine($"[完成] 已停止 {processName}：PID {process.Id}");
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or NotSupportedException)
            {
                Console.WriteLine($"[警告] 无法停止 {processName}（PID {process.Id}）：{ex.Message}");
            }
        }
    }
}

static bool DeleteStartupTask()
{
    try
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "schtasks.exe",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        process.StartInfo.ArgumentList.Add("/Delete");
        process.StartInfo.ArgumentList.Add("/TN");
        process.StartInfo.ArgumentList.Add(AppPaths.TaskName);
        process.StartInfo.ArgumentList.Add("/F");

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            Console.WriteLine($"[完成] 已删除计划任务：{AppPaths.TaskName}");
            return true;
        }

        if (IsTaskMissing(output) || IsTaskMissing(error))
        {
            Console.WriteLine($"[完成] 计划任务不存在，无需删除：{AppPaths.TaskName}");
            return true;
        }

        Console.WriteLine($"[提示] 删除计划任务未成功，可能任务不存在或无权限：{FirstNonEmpty(error, output, "未知错误")}");
        return true;
    }
    catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
    {
        Console.WriteLine($"[提示] 无法调用 schtasks 删除计划任务：{ex.Message}");
        return true;
    }
}

static bool IsTaskMissing(string text)
{
    return text.Contains("cannot find the file", StringComparison.OrdinalIgnoreCase)
        || text.Contains("system cannot find", StringComparison.OrdinalIgnoreCase)
        || text.Contains("找不到", StringComparison.OrdinalIgnoreCase)
        || text.Contains("不存在", StringComparison.OrdinalIgnoreCase);
}

static string FirstNonEmpty(params string[] values)
{
    return values.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
}
