using System;
using System.Linq;
using System.Windows;

namespace TaskManager.App.Services;

public class ThemeService
{
    public void ChangeTheme(string themeName)
    {
        var uri = new Uri($"Themes/{themeName}.xaml", UriKind.Relative);
        var resDict = new ResourceDictionary() { Source = uri };
        
        var existingDicts = Application.Current.Resources.MergedDictionaries
            .Where(d => d.Source != null && d.Source.OriginalString.StartsWith("Themes/"))
            .ToList();
            
        foreach (var d in existingDicts)
        {
            Application.Current.Resources.MergedDictionaries.Remove(d);
        }
        
        Application.Current.Resources.MergedDictionaries.Add(resDict);
    }
}
