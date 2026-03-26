using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Sample.InfinityNavigation.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isActive = (bool)value!;

        var key = isActive ? "PipboyBorderBrush" : "PipboyPrimaryDarkBrush";

        if (Application.Current!.TryFindResource(key, out var resource))
        {
            return resource as IBrush;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
