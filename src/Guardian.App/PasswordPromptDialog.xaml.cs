using System.Windows;

namespace Guardian.App;

public partial class PasswordPromptDialog : Window
{
    private readonly Func<string, bool> _verifyPassword;

    public PasswordPromptDialog(string reason, Func<string, bool> verifyPassword)
    {
        InitializeComponent();
        _verifyPassword = verifyPassword;
        ReasonText.Text = reason;
        Loaded += (_, _) => PasswordInput.Focus();
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (_verifyPassword(PasswordInput.Password))
        {
            DialogResult = true;
            Close();
            return;
        }

        ErrorText.Text = "密码不正确，请重新输入。";
        PasswordInput.Clear();
        PasswordInput.Focus();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
