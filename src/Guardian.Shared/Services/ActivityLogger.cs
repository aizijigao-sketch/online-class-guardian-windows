using System.Text.Json;
using Guardian.Shared.Models;

namespace Guardian.Shared.Services;

public sealed class ActivityLogger(string? logPath = null)
{
    private readonly string _logPath = logPath ?? AppPaths.ActivityLogPath;

    public bool Log(BlockEvent blockEvent)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
            File.AppendAllText(_logPath, JsonSerializer.Serialize(blockEvent) + Environment.NewLine);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
