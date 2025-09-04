using AsyncNavigation;
using AsyncNavigation.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sample.Wpf;

internal class InnerIndicatorProvider : IInnerIndicatorProvider
{
    private IServiceProvider _serviceProvider;
    public InnerIndicatorProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public UIElement GetErrorIndicator(NavigationContext navigationContext)
    {
        return BuildErrorIndicator(_serviceProvider, navigationContext);
    }

    public UIElement GetLoadingIndicator(NavigationContext navigationContext)
    {
        return BuildLoadingIndicator(_serviceProvider, navigationContext);
    }

    public bool HasErrorIndicator(NavigationContext context)
    {
        return true;
    }

    public bool HasLoadingIndicator(NavigationContext context)
    {
        return true;
    }

    private static UIElement BuildLoadingIndicator(IServiceProvider sp, NavigationContext navigationContext)
    {
        var textLoading = new TextBlock
        {
            Text = "Loaing...",
            FontSize = 20,
            Foreground = Brushes.Orange,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        var text = new TextBlock
        {
            Text = navigationContext.ToString(),
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        var bar = new ProgressBar
        {
            IsIndeterminate = true,
            Height = 30,
        };
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Children.Add(textLoading);
        panel.Children.Add(text);
        panel.Children.Add(bar);
        var border = new Border
        {
            Child = panel,
            Padding = new Thickness(5),
            Margin = new Thickness(5),
            Background = Brushes.White,
            BorderThickness = new Thickness(2),
            BorderBrush = Brushes.AliceBlue,
        };
        return border;
    }

    private static UIElement BuildErrorIndicator(IServiceProvider sp, NavigationContext navigationContext)
    {
        var textFailed = new TextBlock
        {
            Text = "Failed",
            FontSize = 20,
            Foreground = Brushes.Red,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        DockPanel.SetDock(textFailed, Dock.Top);
        var error = new TextBlock
        {
            Text = navigationContext.ToString(),
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        var scrollView = new ScrollViewer
        {
            Content = error,
        };
        var panel = new DockPanel
        {
            LastChildFill = true,
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Children.Add(textFailed);
        panel.Children.Add(scrollView);
        var border = new Border
        {
            Child = panel,
            Padding = new Thickness(5),
            Margin = new Thickness(5),
            Background = Brushes.White,
            BorderThickness = new Thickness(2),
            BorderBrush = Brushes.AliceBlue,
        };
        return border;
    }
}
