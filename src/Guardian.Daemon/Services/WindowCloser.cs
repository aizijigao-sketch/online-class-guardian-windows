using System.Runtime.InteropServices;
using System.Text;

namespace Guardian.Daemon.Services;

public sealed class WindowCloser
{
    private const int MaxTitleLength = 512;
    private const uint WmClose = 0x0010;

    public bool CloseWindowsForProcess(int processId)
    {
        var posted = false;
        EnumWindows((handle, _) =>
        {
            if (!IsWindowVisible(handle) || GetProcessId(handle) != processId)
            {
                return true;
            }

            var title = GetTitle(handle);
            if (!string.IsNullOrWhiteSpace(title))
            {
                PostMessage(handle, WmClose, IntPtr.Zero, IntPtr.Zero);
                posted = true;
            }

            return true;
        }, IntPtr.Zero);
        return posted;
    }

    public bool HasVisibleWindow(int processId)
    {
        var found = false;
        EnumWindows((handle, _) =>
        {
            if (IsWindowVisible(handle) && GetProcessId(handle) == processId && !string.IsNullOrWhiteSpace(GetTitle(handle)))
            {
                found = true;
                return false;
            }

            return true;
        }, IntPtr.Zero);
        return found;
    }

    private static int GetProcessId(IntPtr handle)
    {
        GetWindowThreadProcessId(handle, out var processId);
        return (int)processId;
    }

    private static string GetTitle(IntPtr handle)
    {
        var builder = new StringBuilder(MaxTitleLength);
        GetWindowText(handle, builder, builder.Capacity);
        return builder.ToString();
    }

    private delegate bool EnumWindowsProc(IntPtr handle, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr handle);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr handle, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr handle, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);
}
