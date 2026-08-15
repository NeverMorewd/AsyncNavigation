using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace AsyncNavigation.WinUI;

public partial class IconDescriptorConverter : IValueConverter
{
    private readonly IIconResolver<FrameworkElement> _iconResolver;

    public IconDescriptorConverter(IIconResolver<FrameworkElement> iconResolver)
    {
        _iconResolver = iconResolver;
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not IconDescriptor descriptor) return null;

        var size = parameter is double d ? d : 24;
        return _iconResolver.Resolve(descriptor, size)!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}