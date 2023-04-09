using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Desktop.Dialogs;

public partial class SettingDialogWindow : Window
{
    public MainWindow _mainWindow;
    public SettingDialogWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        _mainWindow.ChangeSettings(AddressBox.Text, LoginBox.Text, PasswordBox.Text, ProjectBox.Text);
        this.Close();
    }
}