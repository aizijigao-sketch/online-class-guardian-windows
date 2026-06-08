using System.Text.Json;
using Guardian.Shared.Defaults;
using Guardian.Shared.Models;

namespace Guardian.Shared.Services;

public sealed class ConfigStore(string? configPath = null)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    public string ConfigPath { get; } = configPath ?? AppPaths.ConfigPath;

    public GuardianConfig Load()
    {
        if (!File.Exists(ConfigPath))
        {
            var defaults = DefaultRules.CreateDefaultConfig();
            Save(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<GuardianConfig>(json, JsonOptions) ?? DefaultRules.CreateDefaultConfig();
            MergeDefaultRules(config);
            return config;
        }
        catch
        {
            BackupCorruptConfig();
            var defaults = DefaultRules.CreateDefaultConfig();
            Save(defaults);
            return defaults;
        }
    }

    public void Save(GuardianConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        var tempPath = ConfigPath + ".tmp";
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(tempPath, json);
        if (File.Exists(ConfigPath))
        {
            File.Delete(ConfigPath);
        }
        File.Move(tempPath, ConfigPath);
    }

    private void BackupCorruptConfig()
    {
        Directory.CreateDirectory(AppPaths.BackupsDirectory);
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var backupPath = Path.Combine(AppPaths.BackupsDirectory, $"config-corrupt-{stamp}.json");
        File.Copy(ConfigPath, backupPath, overwrite: true);
    }

    private static void MergeDefaultRules(GuardianConfig config)
    {
        var defaults = DefaultRules.CreateDefaultConfig();
        AddMissing(config.BlockRules.ProcessNames, defaults.BlockRules.ProcessNames);
        AddMissing(config.BlockRules.PathKeywords, defaults.BlockRules.PathKeywords);
        AddMissing(config.BlockRules.WindowTitleKeywords, defaults.BlockRules.WindowTitleKeywords);
        AddMissing(config.AllowRules.ProcessNames, defaults.AllowRules.ProcessNames);
        AddMissing(config.AllowRules.PathKeywords, defaults.AllowRules.PathKeywords);
        AddMissing(config.AllowRules.FileExtensions, defaults.AllowRules.FileExtensions);
        AddMissing(config.SystemProtection.ProtectedProcessNames, defaults.SystemProtection.ProtectedProcessNames);
        RemoveIfPresent(config.BlockRules.ProcessNames, ["v2rayN.exe", "clash-win64.exe", "Clash for Windows.exe", "Shadowsocks.exe"]);
        RemoveIfPresent(config.BlockRules.PathKeywords, ["v2ray", "Clash", "Shadowsocks"]);

        if (config.Notification.Messages.Count == 0)
        {
            AddMissing(config.Notification.Messages, defaults.Notification.Messages);
        }
    }

    private static void AddMissing(List<string> target, IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            if (!target.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                target.Add(value);
            }
        }
    }

    private static void RemoveIfPresent(List<string> target, IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            target.RemoveAll(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase));
        }
    }
}
