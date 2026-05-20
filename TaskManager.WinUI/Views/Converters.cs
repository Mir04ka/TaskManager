using ABI.System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Data.SqlTypes;
using Windows.UI;

namespace TaskManager.WinUI.Views;

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => value is Visibility.Visible;
}

public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => value is Visibility.Collapsed;
}

public sealed class EmptyStringToCollapsedConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value == null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class NullToVisibleConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value == null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
    {
        return (value as string) switch
        {
            "Todo" => new SolidColorBrush(Color.FromArgb(255, 100, 149, 237)),
            "InProgress" => new SolidColorBrush(Color.FromArgb(255, 255, 165, 0)),
            "Done" => new SolidColorBrush(Color.FromArgb(255, 60, 179, 113)),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }
    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class DeadlineColorConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            if (dt.Date < DateTime.Today) return new SolidColorBrush(Color.FromArgb(255, 220, 53, 69));
            if (dt.Date <= DateTime.Today.AddDays(2)) return new SolidColorBrush(Color.FromArgb(255, 255, 140, 0));
        }
        return new SolidColorBrush(Colors.Gray);
    }
    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value is DateTime dt ? dt.ToString("dd.MM.yyyy HH:mm") : string.Empty;
    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class DateOnlyConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value is DateTime dt ? dt.ToString("dd.MM.yyyy") : "—";
    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class BoolToAdminLabelConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value is true ? "Админ" : "Участник";
    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
        => value is bool b ? !b : value;

    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        => value is bool b ? !b : value;
}

public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, System.Type targetType, object parameter, string language)
    {
        if (value is DateTime dt)
        {
            return new System.DateTimeOffset(dt);
        }

        return null;
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, string language)
    {
        if (value is System.DateTimeOffset dto)
        {
            return DateTime.SpecifyKind(dto.UtcDateTime, DateTimeKind.Utc);
        }

        return null;
    }
}