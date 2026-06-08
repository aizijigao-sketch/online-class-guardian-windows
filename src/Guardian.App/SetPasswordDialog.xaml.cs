using System.Windows;

namespace Guardian.App;

public partial class SetPasswordDialog : Window
{
    public string Password { get; private set; } = string.Empty;

    public SetPasswordDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => PasswordInput.Focus();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (PasswordInput.Password.Length < 6)
        {
            ErrorText.Text = "密码至少需要 6 个字符。";
            return;
        }

        if (PasswordInput.Password != ConfirmInput.Password)
        {
            ErrorText.Text = "两次输入的密码不一致。";
            ConfirmInput.Clear();
            ConfirmInput.Focus();
            return;
        }

        Password = PasswordInput.Password;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
