using System.Diagnostics;

namespace Guardian.Daemon.Services;

public sealed class UserNotifier
{
    public void Show(string message)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "msg.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo.ArgumentList.Add(Environment.UserName);
            process.StartInfo.ArgumentList.Add(message);
            process.Start();
        }
        catch
        {
            // Notification is best-effort. Blocking and logging must continue even if msg.exe fails.
        }
    }
}
