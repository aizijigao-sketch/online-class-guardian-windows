namespace Guardian.Shared;

public static class AppPaths
{
    public const string AppName = "OnlineClassGuardian";
    public const string TaskName = "OnlineClassGuardian";
    public const string ServiceName = "OnlineClassGuardianService";
    public const string ServiceDisplayName = "网课守护服务";

    public static string AppDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);

    public static string ConfigPath => Path.Combine(AppDataDirectory, "config.json");

    public static string LogsDirectory => Path.Combine(AppDataDirectory, "logs");

    public static string ActivityLogPath => Path.Combine(LogsDirectory, "activity.log");

    public static string BackupsDirectory => Path.Combine(AppDataDirectory, "backups");
}
