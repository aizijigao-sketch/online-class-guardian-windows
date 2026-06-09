using Guardian.Shared.Services;

namespace Guardian.Shared.Tests;

public sealed class WindowsServiceStatusTests
{
    [Fact]
    public void ExpectedBinaryAndConfig_MatchQuotedServicePath()
    {
        var status = new WindowsServiceStatus(
            true,
            true,
            "\"C:\\Tools\\Guardian.Daemon.exe\" --service --config \"C:\\Users\\parent\\AppData\\Roaming\\OnlineClassGuardian\\config.json\"",
            "RUNNING",
            null);

        Assert.True(status.HasExpectedBinary("C:\\Tools\\Guardian.Daemon.exe"));
        Assert.True(status.HasExpectedConfig("C:\\Users\\parent\\AppData\\Roaming\\OnlineClassGuardian\\config.json"));
    }

    [Fact]
    public void ExpectedConfig_DetectsDifferentConfigPath()
    {
        var status = new WindowsServiceStatus(
            true,
            true,
            "\"C:\\Tools\\Guardian.Daemon.exe\" --service --config \"C:\\Windows\\System32\\config\\systemprofile\\AppData\\Roaming\\OnlineClassGuardian\\config.json\"",
            "RUNNING",
            null);

        Assert.False(status.HasExpectedConfig("C:\\Users\\parent\\AppData\\Roaming\\OnlineClassGuardian\\config.json"));
    }
}
