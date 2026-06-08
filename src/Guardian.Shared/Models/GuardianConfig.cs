namespace Guardian.Shared.Models;

public sealed class GuardianConfig
{
    public LockModeConfig LockMode { get; set; } = new();
    public AuthConfig Auth { get; set; } = new();
    public ProcessProtectionConfig ProcessProtection { get; set; } = new();
    public RuleSet BlockRules { get; set; } = new();
    public RuleSet AllowRules { get; set; } = new();
    public SystemProtectionConfig SystemProtection { get; set; } = new();
    public NotificationConfig Notification { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
}

public sealed class LockModeConfig
{
    public bool Enabled { get; set; }
    public bool StartOnBoot { get; set; } = true;
    public int CheckIntervalSeconds { get; set; } = 2;
    public int StartupGraceSeconds { get; set; } = 20;
    public DateTimeOffset? LastEnabledAt { get; set; }
}

public sealed class AuthConfig
{
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string HashAlgorithm { get; set; } = "PBKDF2-SHA256";
    public int Iterations { get; set; } = 210_000;

    public bool HasPassword => !string.IsNullOrWhiteSpace(PasswordHash) && !string.IsNullOrWhiteSpace(PasswordSalt);
}

public sealed class ProcessProtectionConfig
{
    public bool EnableWatchdog { get; set; } = true;
    public bool RestartCompanionProcess { get; set; } = true;
    public bool RequirePasswordToExitWhenLocked { get; set; } = true;
}

public sealed class RuleSet
{
    public List<string> ProcessNames { get; set; } = [];
    public List<string> PathKeywords { get; set; } = [];
    public List<string> WindowTitleKeywords { get; set; } = [];
    public List<string> FileExtensions { get; set; } = [];
}

public sealed class SystemProtectionConfig
{
    public bool NeverKillWindowsDirectory { get; set; } = true;
    public List<string> ProtectedProcessNames { get; set; } = [];
}

public sealed class NotificationConfig
{
    public bool ShowToast { get; set; } = true;
    public List<string> Messages { get; set; } = [];
}

public sealed class LoggingConfig
{
    public bool Enabled { get; set; } = true;
    public bool LogBlockedApps { get; set; } = true;
    public string LogPath { get; set; } = @"logs\activity.log";
}
