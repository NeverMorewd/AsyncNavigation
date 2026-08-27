using AsyncNavigation;
using AsyncNavigation.Abstractions;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Sample.WinUI;

internal sealed class MessageBoxIndicatorProvider : IRegionIndicatorProvider
{
    private readonly MessageBoxIndicator _indicator = new();
    public bool HasIndicator(string regionName) => true;
    public IRegionIndicator GetIndicator(string regionName) => _indicator;
}

internal sealed class MessageBoxIndicator : IRegionIndicator
{
    private Window? _loadingWindow;

    public Task ShowLoadingAsync(NavigationContext context)
    {
        _loadingWindow = CreateWindow("Loading", context.ToString(), Colors.Orange);
        _loadingWindow.Activate();
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(NavigationContext context, Exception? innerException = null)
    {
        var window = CreateWindow("Navigation error", $"{context}\n\n{innerException}", Colors.Red);
        window.Activate();
        return Task.CompletedTask;
    }

    public Task OnLoadedAsync(NavigationContext context) => CloseLoadingWindow();
    public Task OnCancelledAsync(NavigationContext context) => CloseLoadingWindow();

    private Task CloseLoadingWindow()
    {
        _loadingWindow?.Close();
        _loadingWindow = null;
        return Task.CompletedTask;
    }

    private static Window CreateWindow(string title, string? message, Windows.UI.Color color)
    {
        var window = new Window { Title = title };
        window.Content = new ScrollViewer
        {
            Content = new TextBlock
            {
                Margin = new Thickness(24),
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(color)
            }
        };
        window.AppWindow.Resize(new Windows.Graphics.SizeInt32(480, 240));
        return window;
    }
}
