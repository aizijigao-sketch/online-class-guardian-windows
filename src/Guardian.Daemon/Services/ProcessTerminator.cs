using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Guardian.Daemon.Services;

public sealed record TerminationResult(string ActionTaken, bool Success, string? ErrorMessage);

public sealed class ProcessTerminator
{
    private readonly WindowCloser _windowCloser = new();

    public async Task<TerminationResult> TerminateAsync(Process process, CancellationToken cancellationToken)
    {
        try
        {
            if (process.HasExited)
            {
                return new TerminationResult("AlreadyExited", true, null);
            }

            var serviceResult = await TryStopOwningServiceAsync(process.Id, cancellationToken);
            if (serviceResult?.Success == true)
            {
                return serviceResult;
            }

            if (process.CloseMainWindow())
            {
                var exited = await WaitForExitAsync(process, TimeSpan.FromSeconds(3), cancellationToken);
                if (exited)
                {
                    return new TerminationResult("CloseMainWindow", true, null);
                }
            }

            if (_windowCloser.CloseWindowsForProcess(process.Id))
            {
                await Task.Delay(800, cancellationToken);
                if (!_windowCloser.HasVisibleWindow(process.Id))
                {
                    return new TerminationResult("CloseWindows", true, null);
                }
            }

            try
            {
                process.Kill(entireProcessTree: true);
                await WaitForExitAsync(process, TimeSpan.FromSeconds(3), cancellationToken);
                return new TerminationResult("KillProcessTree", process.HasExited, process.HasExited ? null : "Process did not exit before timeout.");
            }
            catch
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    await WaitForExitAsync(process, TimeSpan.FromSeconds(3), cancellationToken);
                }

                return new TerminationResult("KillProcess", process.HasExited, process.HasExited ? null : "Process did not exit before timeout.");
            }
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or NotSupportedException or AggregateException)
        {
            return new TerminationResult("Failed", false, ex.Message);
        }
    }

    private static async Task<TerminationResult?> TryStopOwningServiceAsync(int processId, CancellationToken cancellationToken)
    {
        var serviceName = await FindServiceNameByProcessIdAsync(processId, cancellationToken);
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            return null;
        }

        var stop = await RunScAsync(["stop", serviceName], cancellationToken);
        for (var attempt = 0; attempt < 10; attempt++)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            var stillRunning = await FindServiceNameByProcessIdAsync(processId, cancellationToken);
            if (string.IsNullOrWhiteSpace(stillRunning))
            {
                return new TerminationResult($"StopService:{serviceName}", true, null);
            }
        }

        var message = string.IsNullOrWhiteSpace(stop.Error) ? stop.Output : stop.Error;
        return new TerminationResult($"StopService:{serviceName}", false, message);
    }

    private static async Task<string?> FindServiceNameByProcessIdAsync(int processId, CancellationToken cancellationToken)
    {
        var result = await RunScAsync(["queryex", "type=", "service", "state=", "all"], cancellationToken);
        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Output))
        {
            return null;
        }

        string? currentService = null;
        foreach (var rawLine in result.Output.Split(Environment.NewLine))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("SERVICE_NAME:", StringComparison.OrdinalIgnoreCase))
            {
                currentService = line["SERVICE_NAME:".Length..].Trim();
                continue;
            }

            var match = Regex.Match(line, @"^PID\s*:\s*(\d+)$", RegexOptions.IgnoreCase);
            if (match.Success &&
                currentService is not null &&
                int.TryParse(match.Groups[1].Value, out var pid) &&
                pid == processId)
            {
                return currentService;
            }
        }

        return null;
    }

    private static async Task<CommandResult> RunScAsync(string[] arguments, CancellationToken cancellationToken)
    {
        using var command = new Process();
        command.StartInfo = new ProcessStartInfo
        {
            FileName = "sc.exe",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            command.StartInfo.ArgumentList.Add(argument);
        }

        command.Start();
        var outputTask = command.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = command.StandardError.ReadToEndAsync(cancellationToken);
        await command.WaitForExitAsync(cancellationToken);
        return new CommandResult(command.ExitCode, await outputTask, await errorTask);
    }

    private static async Task<bool> WaitForExitAsync(Process process, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var waitTask = process.WaitForExitAsync(cancellationToken);
        var delayTask = Task.Delay(timeout, cancellationToken);
        var completed = await Task.WhenAny(waitTask, delayTask);
        return completed == waitTask && process.HasExited;
    }

    private sealed record CommandResult(int ExitCode, string Output, string Error);
}
