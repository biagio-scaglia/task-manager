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
    public static readonly IValueConverter EnumToVisibilityConverter = new EnumToVisibilityConverterImpl();
    public static readonly IValueConverter EnumToActiveForegroundConverter = new EnumToActiveForegroundConverterImpl();
    public static readonly IValueConverter InverseBooleanToVisibilityConverter = new InverseBooleanToVisibilityConverterImpl();
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

public class InverseBooleanToVisibilityConverterImpl : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible && !isVisible)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class EnumToVisibilityConverterImpl : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return Visibility.Collapsed;
        return value.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class EnumToActiveForegroundConverterImpl : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")); // Pure White

        if (value.ToString() == parameter.ToString())
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F0F0F")); // Deep Black
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")); // Pure White
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
