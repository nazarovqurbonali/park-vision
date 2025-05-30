using System.Windows;

namespace WpfApp;

public class App(MainWindow mainWindow) : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        mainWindow.Show();
        base.OnStartup(e);
    }
}