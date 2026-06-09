using Guardian.Daemon.Services;
using Guardian.Shared;
using Guardian.Shared.Services;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

try
{
    var options = DaemonOptions.Parse(args);
    var serviceManager = new WindowsServiceManager();
    var handledServiceCommand = false;

    if (options.InstallService)
    {
        var install = serviceManager.InstallOrRepair(GetCurrentExecutablePath(), options.ConfigPath);
        Console.WriteLine(install.Message);
        if (!install.Success)
        {
            Environment.ExitCode = 1;
            return;
        }

        handledServiceCommand = true;
    }

    if (options.StartService)
    {
        var start = serviceManager.Start();
        Console.WriteLine(start.Message);
        if (!start.Success)
        {
            Environment.ExitCode = 1;
            return;
        }

        handledServiceCommand = true;
    }

    if (options.RemoveService)
    {
        var remove = serviceManager.Remove();
        Console.WriteLine(remove.Message);
        Environment.ExitCode = remove.Success ? 0 : 1;
        return;
    }

    if (options.StatusService)
    {
        var status = serviceManager.Query();
        Console.WriteLine(status.Exists
            ? $"服务状态：{status.State}; 路径：{status.BinaryPath}"
            : $"服务未安装：{status.ErrorMessage}");
        return;
    }

    if (handledServiceCommand && !options.RunAsService && !options.RunOnce)
    {
        return;
    }

    var monitor = CreateMonitor(options.ConfigPath);
    if (options.RunOnce)
    {
        await monitor.RunOnceAsync();
        return;
    }

    if (!options.RunAsService)
    {
        await monitor.RunAsync(CancellationToken.None);
        return;
    }

    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddSingleton(new ConfigStore(options.ConfigPath));
    builder.Services.AddSingleton<RuleMatcher>();
    builder.Services.AddSingleton<ProcessTerminator>();
    builder.Services.AddSingleton<ActivityLogger>();
    builder.Services.AddSingleton<ReminderPicker>();
    builder.Services.AddSingleton<CompanionWatchdog>();
    builder.Services.AddSingleton<UserNotifier>();
    builder.Services.AddSingleton<ProcessMonitor>();
    builder.Services.AddHostedService<GuardianWorker>();
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = AppPaths.ServiceName;
    });

    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Directory.CreateDirectory(AppPaths.LogsDirectory);
    var logPath = Path.Combine(AppPaths.LogsDirectory, "daemon-error.log");
    File.AppendAllText(logPath, $"{DateTimeOffset.Now:O} {ex}{Environment.NewLine}");
}

static ProcessMonitor CreateMonitor(string configPath) =>
    new(
        new ConfigStore(configPath),
        new RuleMatcher(),
        new ProcessTerminator(),
        new ActivityLogger(),
        new ReminderPicker(),
        new CompanionWatchdog(),
        new UserNotifier());

static string GetCurrentExecutablePath() =>
    Environment.ProcessPath
    ?? Process.GetCurrentProcess().MainModule?.FileName
    ?? Path.Combine(AppContext.BaseDirectory, "Guardian.Daemon.exe");

internal sealed record DaemonOptions(
    bool RunOnce,
    bool RunAsService,
    bool InstallService,
    bool StartService,
    bool RemoveService,
    bool StatusService,
    string ConfigPath)
{
    public static DaemonOptions Parse(string[] args)
    {
        var configPath = AppPaths.ConfigPath;

        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--config", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                configPath = args[i + 1];
                i++;
            }
        }

        return new DaemonOptions(
            Has(args, "--once"),
            Has(args, "--service"),
            Has(args, "--install-service"),
            Has(args, "--start-service"),
            Has(args, "--remove-service"),
            Has(args, "--status-service"),
            configPath);
    }

    private static bool Has(string[] args, string name) =>
        args.Any(arg => string.Equals(arg, name, StringComparison.OrdinalIgnoreCase));
}
