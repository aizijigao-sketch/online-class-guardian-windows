using Microsoft.Extensions.Hosting;

namespace Guardian.Daemon.Services;

public sealed class GuardianWorker(ProcessMonitor monitor) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        monitor.RunAsync(stoppingToken);
}
