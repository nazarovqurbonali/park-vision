using System.Windows;

namespace WpfApp.Services;

public class ThemeManager
{
    private const string LightThemePath = "Themes/LightTheme.xaml";
    private const string DarkThemePath = "Themes/DarkTheme.xaml";

    private const string LightThemeFile = "LightTheme.xaml";
    private const string DarkThemeFile = "DarkTheme.xaml";

    public void SetTheme(bool isDark)
    {
        Uri dictUri = new(isDark ? DarkThemePath : LightThemePath, UriKind.Relative);
        ResourceDictionary themeDict = new() { Source = dictUri };

        var mergedDicts = System.Windows.Application.Current.Resources.MergedDictionaries;

        for (int i = mergedDicts.Count - 1; i >= 0; i--)
        {
            ResourceDictionary md = mergedDicts[i];
            if (md.Source != null &&
                (md.Source.OriginalString.EndsWith(LightThemeFile, StringComparison.OrdinalIgnoreCase) ||
                 md.Source.OriginalString.EndsWith(DarkThemeFile, StringComparison.OrdinalIgnoreCase)))
            {
                mergedDicts.RemoveAt(i);
            }
        }

        mergedDicts.Add(themeDict);
    }
}