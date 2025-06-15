using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.Services;

namespace WpfApp.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    private static readonly ThemeManager ThemeManager = new();
    private bool _isDarkTheme;

    public ICommand ToggleThemeCommand { get; set; }

    public MainWindowViewModel()
    {
        ToggleThemeCommand = new RelayCommand(ToggleTheme);

        ThemeManager.SetTheme(_isDarkTheme);
    }

    private void ToggleTheme()
    {
        _isDarkTheme = !_isDarkTheme;
        ThemeManager.SetTheme(_isDarkTheme);
    }
}