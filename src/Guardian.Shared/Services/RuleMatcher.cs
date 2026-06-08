using Guardian.Shared.Models;

namespace Guardian.Shared.Services;

public sealed class RuleMatcher
{
    public RuleDecision Decide(ProcessSnapshot snapshot, GuardianConfig config)
    {
        var processName = Normalize(snapshot.ProcessName);
        var path = snapshot.MainModulePath ?? string.Empty;

        if (IsMatch(processName, config.SystemProtection.ProtectedProcessNames))
        {
            return RuleDecision.Ignore("系统保护进程");
        }

        if (config.SystemProtection.NeverKillWindowsDirectory && IsWindowsDirectory(path))
        {
            return RuleDecision.Ignore("Windows 系统目录进程");
        }

        if (IsMatch(processName, config.AllowRules.ProcessNames) || ContainsKeyword(path, config.AllowRules.PathKeywords))
        {
            return RuleDecision.Allow("允许名单");
        }

        if (IsMatch(processName, config.BlockRules.ProcessNames))
        {
            return RuleDecision.Block("禁止进程名");
        }

        if (ContainsKeyword(path, config.BlockRules.PathKeywords) ||
            ContainsKeyword(snapshot.MainWindowTitle ?? string.Empty, config.BlockRules.WindowTitleKeywords))
        {
            return RuleDecision.Block("禁止关键词");
        }

        return RuleDecision.Ignore("未命中规则");
    }

    private static bool IsMatch(string value, IEnumerable<string> candidates) =>
        candidates.Any(candidate => string.Equals(value, Normalize(candidate), StringComparison.OrdinalIgnoreCase));

    private static bool ContainsKeyword(string value, IEnumerable<string> keywords) =>
        keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));

    private static bool IsWindowsDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        return path.StartsWith(Path.Combine(windows, "System32"), StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith(Path.Combine(windows, "SysWOW64"), StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string processName) =>
        processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? processName : processName + ".exe";
}
