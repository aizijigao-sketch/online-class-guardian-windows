using Guardian.Shared.Models;
using Guardian.Shared.Services;

namespace Guardian.Shared.Tests;

public sealed class ActivityLoggerTests
{
    [Fact]
    public void Log_AppendsJsonLine()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "activity.log");
        var logger = new ActivityLogger(path);

        Assert.True(logger.Log(new BlockEvent { ProcessName = "chrome.exe", ProcessId = 42, Success = true }));
        Assert.Contains("chrome.exe", File.ReadAllText(path));
    }
}
