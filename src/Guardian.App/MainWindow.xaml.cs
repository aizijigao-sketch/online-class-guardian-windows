using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Guardian.Shared;
using Guardian.Shared.Models;
using Guardian.Shared.Services;
using Forms = System.Windows.Forms;
using MediaColor = System.Windows.Media.Color;

namespace Guardian.App;

public partial class MainWindow : Window
{
    private readonly ConfigStore _configStore;
    private readonly PasswordHasher _passwordHasher;
    private readonly DaemonSupervisor _daemonSupervisor = new();
    private readonly DispatcherTimer _daemonTimer = new();
    private GuardianConfig _config;
    private Forms.NotifyIcon? _notifyIcon;
    private bool _isExitRequested;

    public MainWindow(ConfigStore configStore, PasswordHasher passwordHasher)
    {
        _configStore = configStore;
        _passwordHasher = passwordHasher;
        _config = _configStore.Load();

        InitializeComponent();
        InitializeTrayIcon();
        ConfigureDaemonTimer();
        RefreshUi();
        EnsureDaemon();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExitRequested)
        {
            e.Cancel = true;
            Hide();
            _notifyIcon?.ShowBalloonTip(1600, "网课守护", "程序已最小化到托盘并继续运行。", Forms.ToolTipIcon.Info);
            return;
        }

        _notifyIcon?.Dispose();
        base.OnClosing(e);
    }

    private void InitializeTrayIcon()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("打开控制面板", null, (_, _) => ShowMainWindow());
        menu.Items.Add("开启网课模式", null, (_, _) => EnableLockMode());
        menu.Items.Add("关闭网课模式", null, (_, _) => DisableLockModeWithPassword());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("退出程序", null, (_, _) => ExitApplication());

        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "网课守护",
            Icon = SystemIcons.Shield,
            ContextMenuStrip = menu,
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    private void ConfigureDaemonTimer()
    {
        _daemonTimer.Interval = TimeSpan.FromSeconds(8);
        _daemonTimer.Tick += (_, _) =>
        {
            ReloadConfig();
            if (_config.LockMode.Enabled && _config.ProcessProtection.RestartCompanionProcess)
            {
                EnsureDaemon();
            }
        };
        _daemonTimer.Start();
    }

    private void EnsureDaemon()
    {
        _daemonSupervisor.TryEnsureDaemonRunning(out var message);
        DaemonStatusText.Text = $"守护进程：{message}";
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        ReloadConfig();
        if (_config.LockMode.Enabled)
        {
            DisableLockModeWithPassword();
        }
        else
        {
            EnableLockMode();
        }
    }

    private void EnableLockMode()
    {
        ReloadConfig();
        _config.LockMode.Enabled = true;
        _config.LockMode.LastEnabledAt = DateTimeOffset.Now;
        _configStore.Save(_config);
        EnsureDaemon();
        RefreshUi();
        _notifyIcon?.ShowBalloonTip(1800, "网课模式已开启", "现在会拦截浏览器、游戏、聊天、音乐和下载软件。", Forms.ToolTipIcon.Info);
    }

    private void DisableLockModeWithPassword()
    {
        ReloadConfig();
        if (!_config.LockMode.Enabled)
        {
            RefreshUi();
            return;
        }

        if (!PromptForPassword("关闭网课模式需要家长密码。"))
        {
            return;
        }

        _config.LockMode.Enabled = false;
        _configStore.Save(_config);
        RefreshUi();
        _notifyIcon?.ShowBalloonTip(1800, "网课模式已关闭", "当前不会拦截应用。", Forms.ToolTipIcon.Info);
    }

    private bool PromptForPassword(string reason)
    {
        var dialog = new PasswordPromptDialog(reason, password => _passwordHasher.Verify(password, _config.Auth))
        {
            Owner = this
        };
        return dialog.ShowDialog() == true;
    }

    private void RecentLogs_Click(object sender, RoutedEventArgs e)
    {
        var message = $"拦截日志位置：{AppPaths.ActivityLogPath}{Environment.NewLine}{Environment.NewLine}日志查看器会在后续版本补完整。";
        System.Windows.MessageBox.Show(this, message, "最近拦截记录", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ExitApplication()
    {
        ReloadConfig();
        if (_config.LockMode.Enabled && _config.ProcessProtection.RequirePasswordToExitWhenLocked)
        {
            ShowMainWindow();
            if (!PromptForPassword("网课模式开启时，退出程序需要家长密码。"))
            {
                return;
            }
        }

        _isExitRequested = true;
        _daemonTimer.Stop();
        _notifyIcon?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    private void ShowMainWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        RefreshUi();
    }

    private void ReloadConfig()
    {
        _config = _configStore.Load();
    }

    private void RefreshUi()
    {
        ReloadConfig();
        var enabled = _config.LockMode.Enabled;

        StatusBadgeText.Text = enabled ? "网课模式：开启" : "网课模式：关闭";
        StatusBadge.Background = enabled
            ? new SolidColorBrush(MediaColor.FromRgb(232, 246, 238))
            : new SolidColorBrush(MediaColor.FromRgb(238, 241, 245));
        StatusBadgeText.Foreground = enabled
            ? new SolidColorBrush(MediaColor.FromRgb(21, 97, 59))
            : new SolidColorBrush(MediaColor.FromRgb(83, 93, 108));

        ModeTitle.Text = enabled ? "当前已开启网课模式" : "当前未开启网课模式";
        ModeDescription.Text = enabled
            ? $"从 {_config.LockMode.LastEnabledAt?.LocalDateTime:yyyy-MM-dd HH:mm} 开始守护。关闭或退出需要输入家长密码。"
            : "点击开启后，状态会保存到本机配置；即使重启，登录后也会继续生效，直到家长输入密码关闭。";
        ToggleButton.Content = enabled ? "关闭网课模式" : "开启网课模式";
        FooterText.Text = enabled
            ? "关闭窗口不会退出程序，托盘会继续守护。"
            : "首次运行已设置家长密码；建议上课前手动开启。";
        DetailPanel.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
        FooterPanel.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
        DaemonStatusText.Text = _daemonSupervisor.IsDaemonRunning()
            ? "守护进程：正在运行"
            : "守护进程：未检测到";

        if (_notifyIcon is not null)
        {
            _notifyIcon.Text = enabled ? "网课守护：已开启" : "网课守护：未开启";
        }
    }
}
