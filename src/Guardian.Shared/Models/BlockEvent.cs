namespace Guardian.Shared.Models;

public sealed class BlockEvent
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
    public string ProcessName { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public string? ProcessPath { get; set; }
    public string DecisionReason { get; set; } = string.Empty;
    public string ActionTaken { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string ReminderMessage { get; set; } = string.Empty;
}
