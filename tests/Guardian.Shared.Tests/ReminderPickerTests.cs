using Guardian.Shared.Defaults;
using Guardian.Shared.Models;
using Guardian.Shared.Services;

namespace Guardian.Shared.Tests;

public sealed class ReminderPickerTests
{
    [Fact]
    public void Pick_ReturnsConfiguredMessage()
    {
        var config = DefaultRules.CreateDefaultConfig();
        config.Notification.Messages = ["加油"];

        Assert.Equal("加油", new ReminderPicker().Pick(config));
    }

    [Fact]
    public void Pick_ReturnsFallbackWhenEmpty()
    {
        var config = new GuardianConfig();

        Assert.NotEmpty(new ReminderPicker().Pick(config));
    }
}
