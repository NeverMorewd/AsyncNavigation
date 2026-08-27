using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;

namespace AsyncNavigation.WinUI;

public class IconResolver : IIconResolver<FrameworkElement>
{
    private readonly Dictionary<string, BitmapImage> _fileCache = [];

    public FrameworkElement? Resolve(IconDescriptor descriptor, double size = 24)
    {
        return descriptor.Kind switch
        {
            IconKind.FilePath => ResolveFile(descriptor.Value, size),
            IconKind.PathData => ResolvePathData(descriptor.Value, size),
            IconKind.IconFont => ResolveThemeIcon(descriptor.Value, size),
            IconKind.ResourceKey => ResolveResourceKey(descriptor.Value, size),
            _ => null
        };
    }

    protected virtual Image? ResolveFile(string path, double size)
    {
        var fullPath = System.IO.Path.Combine(AppContext.BaseDirectory, path);
        if (!System.IO.File.Exists(fullPath)) return null;

        if (!_fileCache.TryGetValue(fullPath, out var bitmap))
        {
            bitmap = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
            _fileCache[fullPath] = bitmap;
        }

        return new Image
        {
            Source = bitmap,
            Width = size,
            Height = size
        };
    }

    protected virtual Microsoft.UI.Xaml.Shapes.Path? ResolvePathData(string data, double size)
    {
        if (string.IsNullOrWhiteSpace(data)) return null;

        var geometry = (Geometry)XamlReader.Load($"<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>{data}</Geometry>");
        var path = new Microsoft.UI.Xaml.Shapes.Path
        {
            Data = geometry,
            Width = size,
            Height = size,
            Stretch = Stretch.Uniform,
            Fill = ResolvePathFill(),
        };

        return path;
    }

    protected virtual TextBlock ResolveThemeIcon(string name, double size)
    {
        return new TextBlock
        {
            Text = name,
            FontSize = size,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
    }

    protected virtual FrameworkElement? ResolveResourceKey(string resourceKey, double size)
    {
        if (Application.Current is null) return null;

        if (!Application.Current.Resources.TryGetValue(resourceKey, out var resource)) return null;

        return resource switch
        {
            Geometry geometry => new Microsoft.UI.Xaml.Shapes.Path
            {
                Data = geometry,
                Width = size,
                Height = size,
                Stretch = Stretch.Uniform,
                Fill = ResolvePathFill(),
            },
            ImageSource imageSource => new Image
            {
                Source = imageSource,
                Width = size,
                Height = size
            },
            string pathData => ResolvePathData(pathData, size),
            _ => null
        };
    }

    private static Brush ResolvePathFill()
    {
        if (Application.Current?.Resources.TryGetValue("TextFillColorPrimaryBrush", out var resource) == true
            && resource is Brush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Microsoft.UI.Colors.Black);
    }
}
