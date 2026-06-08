using Guardian.Daemon.Services;
using Guardian.Shared;
using Guardian.Shared.Services;

try
{
    var once = args.Any(static arg => string.Equals(arg, "--once", StringComparison.OrdinalIgnoreCase));
    var configStore = new ConfigStore();
    var matcher = new RuleMatcher();
    var terminator = new ProcessTerminator();
    var logger = new ActivityLogger();
    var reminders = new ReminderPicker();
    var watchdog = new CompanionWatchdog();
    var notifier = new UserNotifier();
    var monitor = new ProcessMonitor(configStore, matcher, terminator, logger, reminders, watchdog, notifier);

    if (once)
    {
        await monitor.RunOnceAsync();
        return;
    }

    await monitor.RunAsync(CancellationToken.None);
}
catch (Exception ex)
{
    Directory.CreateDirectory(AppPaths.LogsDirectory);
    var logPath = Path.Combine(AppPaths.LogsDirectory, "daemon-error.log");
    File.AppendAllText(logPath, $"{DateTimeOffset.Now:O} {ex}{Environment.NewLine}");
}
