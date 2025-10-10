using AsyncNavigation;
using AsyncNavigation.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Sample.Avalonia;

internal class InnerIndicatorProvider : IInnerIndicatorProvider
{
    private readonly IServiceProvider _serviceProvider;
    public InnerIndicatorProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public Control GetErrorIndicator(NavigationContext navigationContext)
    {
        return BuildErrorIndicator(_serviceProvider, navigationContext);
    }

    public Control GetLoadingIndicator(NavigationContext navigationContext)
    {
        return BuildLoadingIndicator(_serviceProvider, navigationContext);
    }

    public bool HasErrorIndicator(NavigationContext navigationContext)
    {
        return true;
    }

    public bool HasLoadingIndicator(NavigationContext navigationContext)
    {
        return true;
    }

    private static Control BuildLoadingIndicator(IServiceProvider sp, NavigationContext navigationContext)
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

        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 100,
            Margin = new Thickness(0, 10, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        cancelButton.Click += async (s, e) =>
        {
            cancelButton.IsEnabled = false;
            cancelButton.Content = "Cancelling...";
            await navigationContext.CancelAndWaitAsync();
        };


        panel.Children.Add(textLoading);
        panel.Children.Add(text);
        panel.Children.Add(bar);
        panel.Children.Add(cancelButton);
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

    private static Control BuildErrorIndicator(IServiceProvider sp, NavigationContext navigationContext)
    {
        var textFailed = new TextBlock
        {
            Text = "Failed",
            FontSize = 20,
            Foreground = Brushes.Red,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        DockPanel.SetDock(textFailed, Dock.Top);
        var error = new SelectableTextBlock
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
