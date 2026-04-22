using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using System.Globalization;

namespace AsyncNavigation.Avalonia;

public class IconDescriptorConverter : IValueConverter
{
    private readonly IIconResolver<Control> _iconResolver;

    public IconDescriptorConverter(IIconResolver<Control> iconResolver)
    {
        _iconResolver = iconResolver;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IconDescriptor descriptor) return null;

        var size = parameter is double d ? d : 24;
        return _iconResolver.Resolve(descriptor, size);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
