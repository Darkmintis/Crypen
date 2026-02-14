using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Crypen.Converters;

/// <summary>
/// Converter that transforms a status string to visibility
/// </summary>
public class StatusToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string status && parameter is string expected)
        {
            return status == expected ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ConvertBack is not used in one-way bindings
        return null!;
    }
}

/// <summary>
/// Converter that transforms an empty collection to visibility
/// </summary>
public class EmptyCollectionToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ConvertBack is not used in one-way bindings
        return null!;
    }
}

/// <summary>
/// Converter that inverts boolean values
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            bool invert = parameter?.ToString()?.ToLowerInvariant() == "invert";
            bool result = invert ? !boolValue : boolValue;
            return result ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            bool invert = parameter?.ToString()?.ToLowerInvariant() == "invert";
            bool result = visibility == Visibility.Visible;
            return invert ? !result : result;
        }
        
        return false;
    }
}

/// <summary>
/// Converter for file size formatting
/// </summary>
public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;
            
            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }
            
            return $"{size:0.##} {suffixes[suffixIndex]}";
        }
        
        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // ConvertBack is not used
        return 0L;
    }
}

/// <summary>
/// Converter for date formatting
/// </summary>
public class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("g"); // Short date and time pattern
        }
        
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string strValue && DateTime.TryParse(strValue, out DateTime result))
        {
            return result;
        }
        
        return DateTime.MinValue;
    }
}
