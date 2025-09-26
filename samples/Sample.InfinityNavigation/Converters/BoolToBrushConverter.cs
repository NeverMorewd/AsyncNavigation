using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Sample.InfinityNavigation.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isActvie = (bool)value!;
        return isActvie ? Brushes.Orange : Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
