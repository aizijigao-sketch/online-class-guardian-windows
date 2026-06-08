namespace Guardian.Shared.Models;

public sealed record ProcessSnapshot(
    int ProcessId,
    string ProcessName,
    string? MainModulePath,
    string? MainWindowTitle);
