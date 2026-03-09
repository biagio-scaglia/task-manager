using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TaskManager.App;

public static class Converters
{
    public static readonly IValueConverter ActiveForegroundConverter = new ActiveForegroundConverterImpl();
    public static readonly IValueConverter BooleanToVisibilityConverter = new BooleanToVisibilityConverterImpl();
}

public class ActiveForegroundConverterImpl : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible && isVisible)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F0F0F")); // Deep Black
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")); // Pure White
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BooleanToVisibilityConverterImpl : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible && isVisible)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
