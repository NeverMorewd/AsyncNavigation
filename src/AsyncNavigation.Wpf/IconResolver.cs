using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AsyncNavigation.Wpf;

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

    protected virtual Path? ResolvePathData(string data, double size)
    {
        if (string.IsNullOrWhiteSpace(data)) return null;

        var path = new Path
        {
            Data = Geometry.Parse(data),
            Width = size,
            Height = size,
            Stretch = Stretch.Uniform,
        };

        // Bind Fill to ancestor Foreground so it follows theme color automatically
        path.SetBinding(Path.FillProperty, new Binding("Foreground")
        {
            RelativeSource = new RelativeSource(
                RelativeSourceMode.FindAncestor,
                typeof(Control),
                1)
        });

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

        var resource = Application.Current.TryFindResource(resourceKey);
        if (resource is null) return null;

        return resource switch
        {
            Geometry geometry => new Path
            {
                Data = geometry,
                Width = size,
                Height = size,
                Stretch = Stretch.Uniform,
            },
            DrawingImage drawing => new Image
            {
                Source = drawing,
                Width = size,
                Height = size
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
}
