using AsyncNavigation;
using AsyncNavigation.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Sample.WinUI;

internal sealed class InnerIndicatorProvider : IInnerIndicatorProvider
{
    public bool HasErrorIndicator(NavigationContext context) => true;
    public bool HasLoadingIndicator(NavigationContext context) => true;

    public UIElement GetLoadingIndicator(NavigationContext context)
    {
        var cancel = new Button { Content = "Cancel", HorizontalAlignment = HorizontalAlignment.Center };
        cancel.Click += async (_, _) =>
        {
            cancel.IsEnabled = false;
            cancel.Content = "Cancelling…";
            await context.CancelAndWaitAsync();
        };
        var panel = new StackPanel { Spacing = 12, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(new TextBlock { Text = "Loading…", FontSize = 24, HorizontalAlignment = HorizontalAlignment.Center });
        panel.Children.Add(new TextBlock { Text = context.ToString(), TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center });
        panel.Children.Add(new ProgressBar { IsIndeterminate = true, Width = 260 });
        panel.Children.Add(cancel);
        return new Border { Padding = new Thickness(24), Child = panel };
    }

    public UIElement GetErrorIndicator(NavigationContext context)
    {
        var panel = new StackPanel { Spacing = 12, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(new TextBlock { Text = "Navigation failed", FontSize = 24, Foreground = new SolidColorBrush(Colors.Red), HorizontalAlignment = HorizontalAlignment.Center });
        panel.Children.Add(new TextBlock { Text = context.ToString(), TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center });
        return new Border { Padding = new Thickness(24), Child = panel };
    }
}
