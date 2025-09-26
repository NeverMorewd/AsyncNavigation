using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Sample.Wpf.InfinityNavigation.Converters;

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
