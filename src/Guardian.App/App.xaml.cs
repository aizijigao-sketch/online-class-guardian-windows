using System.Windows;
using Guardian.Shared.Services;

namespace Guardian.App;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configStore = new ConfigStore();
        var passwordHasher = new PasswordHasher();
        var config = configStore.Load();

        if (!config.Auth.HasPassword)
        {
            var dialog = new SetPasswordDialog();
            if (dialog.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            config.Auth = passwordHasher.HashPassword(dialog.Password);
            configStore.Save(config);
        }

        var mainWindow = new MainWindow(configStore, passwordHasher);
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
