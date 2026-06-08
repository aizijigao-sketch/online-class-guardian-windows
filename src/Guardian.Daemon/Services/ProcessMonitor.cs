using System.Diagnostics;
using Guardian.Shared.Models;
using Guardian.Shared.Services;

namespace Guardian.Daemon.Services;

public sealed class ProcessMonitor(
    ConfigStore configStore,
    RuleMatcher matcher,
    ProcessTerminator terminator,
    ActivityLogger logger,
    ReminderPicker reminderPicker,
    CompanionWatchdog watchdog,
    UserNotifier notifier)
{
    private static readonly string[] SelfProcessNames = ["Guardian.Daemon", "Guardian.App", "Guardian.Recovery"];
    private readonly DateTimeOffset _startedAt = DateTimeOffset.Now;
    private bool _startupGraceAnnounced;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var config = configStore.Load();
            await RunCycleAsync(config, cancellationToken);
            var interval = Math.Clamp(config.LockMode.CheckIntervalSeconds, 1, 30);
            await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);
        }
    }

    public async Task RunOnceAsync()
    {
        var config = configStore.Load();
        await RunCycleAsync(config, CancellationToken.None, ignoreGracePeriod: true);
    }

    private async Task RunCycleAsync(GuardianConfig config, CancellationToken cancellationToken, bool ignoreGracePeriod = false)
    {
        if (!config.LockMode.Enabled)
        {
            return;
        }

        if (!ignoreGracePeriod && IsInStartupGracePeriod(config))
        {
            if (!_startupGraceAnnounced)
            {
                _startupGraceAnnounced = true;
            }
            return;
        }

        if (config.ProcessProtection.RestartCompanionProcess)
        {
            watchdog.EnsureAppRunning();
        }
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    await InspectProcessAsync(process, config, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.Log(new BlockEvent
                    {
                        Timestamp = DateTimeOffset.Now,
                        ProcessName = SafeProcessName(process),
                        ProcessId = SafeProcessId(process),
                        DecisionReason = "巡检异常",
                        ActionTaken = "InspectProcess",
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
        }
    }

    private async Task InspectProcessAsync(Process process, GuardianConfig config, CancellationToken cancellationToken)
    {
        ProcessSnapshot snapshot;
        try
        {
            if (process.HasExited || SelfProcessNames.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            snapshot = new ProcessSnapshot(process.Id, process.ProcessName, TryGetPath(process), TryGetTitle(process));
        }
        catch
        {
            return;
        }

        var decision = matcher.Decide(snapshot, config);
        if (decision.Kind != RuleDecisionKind.Block)
        {
            return;
        }

        var reminder = reminderPicker.Pick(config);
        var result = await terminator.TerminateAsync(process, cancellationToken);
        if (result.Success && config.Notification.ShowToast)
        {
            notifier.Show(reminder);
        }
        logger.Log(new BlockEvent
        {
            Timestamp = DateTimeOffset.Now,
            ProcessId = snapshot.ProcessId,
            ProcessName = snapshot.ProcessName,
            ProcessPath = snapshot.MainModulePath,
            DecisionReason = decision.Reason,
            ActionTaken = result.ActionTaken,
            Success = result.Success,
            ErrorMessage = result.ErrorMessage,
            ReminderMessage = reminder
        });
    }

    private bool IsInStartupGracePeriod(GuardianConfig config)
    {
        var grace = Math.Clamp(config.LockMode.StartupGraceSeconds, 0, 120);
        return DateTimeOffset.Now - _startedAt < TimeSpan.FromSeconds(grace);
    }

    private static string? TryGetPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryGetTitle(Process process)
    {
        try
        {
            return process.MainWindowTitle;
        }
        catch
        {
            return null;
        }
    }

    private static string SafeProcessName(Process process)
    {
        try
        {
            return process.ProcessName;
        }
        catch
        {
            return "unknown";
        }
    }

    private static int SafeProcessId(Process process)
    {
        try
        {
            return process.Id;
        }
        catch
        {
            return 0;
        }
    }
}
