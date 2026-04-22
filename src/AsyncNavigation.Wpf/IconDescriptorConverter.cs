using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AsyncNavigation.Wpf;

public class IconDescriptorConverter : IValueConverter
{
    private readonly IIconResolver<FrameworkElement> _iconResolver;

    public IconDescriptorConverter(IIconResolver<FrameworkElement> iconResolver)
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