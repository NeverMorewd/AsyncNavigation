using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Sample.InfinityNavigation.Converters;

public class NumberToDockConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int number)
        {
            int index = number % 4;
            return index switch
            {
                0 => Dock.Left,
                1 => Dock.Top,
                2 => Dock.Right,
                3 => Dock.Bottom,
                _ => Dock.Left
            };
        }
        return Dock.Left;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
