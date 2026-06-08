using Guardian.Shared.Services;

namespace Guardian.Shared.Tests;

public sealed class ConfigStoreTests
{
    [Fact]
    public void Load_CreatesDefaultConfigWhenMissing()
    {
        var path = NewTempConfigPath();
        var store = new ConfigStore(path);

        var config = store.Load();

        Assert.True(File.Exists(path));
        Assert.Contains("chrome.exe", config.BlockRules.ProcessNames);
    }

    [Fact]
    public void SaveThenLoad_PreservesLockMode()
    {
        var path = NewTempConfigPath();
        var store = new ConfigStore(path);
        var config = store.Load();
        config.LockMode.Enabled = true;

        store.Save(config);
        var loaded = store.Load();

        Assert.True(loaded.LockMode.Enabled);
    }

    [Fact]
    public void Load_CorruptConfigReturnsDefault()
    {
        var path = NewTempConfigPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{ not-json");
        var store = new ConfigStore(path);

        var config = store.Load();

        Assert.Contains("chrome.exe", config.BlockRules.ProcessNames);
    }

    private static string NewTempConfigPath() =>
        Path.Combine(Path.GetTempPath(), "OnlineClassGuardianTests", Guid.NewGuid().ToString("N"), "config.json");
}
