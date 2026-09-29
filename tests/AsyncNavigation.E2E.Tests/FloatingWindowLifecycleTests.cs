using AsyncNavigation.Avalonia.Floating;
using AsyncNavigation.Floating;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;

namespace AsyncNavigation.E2E.Tests;

public sealed class FloatingWindowLifecycleTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dock_and_close_raise_separate_requests(bool dock)
    {
        var content = new Border();
        var restoreRequested = false;
        var closeRequested = false;
        var host = new AvaloniaFloatingWindowHost(new FloatingWindowOptions
        {
            Title = "Floating lifecycle test",
            Width = 320,
            Height = 240
        });

        host.SetContentAsync(content).GetAwaiter().GetResult();
        host.ShowAsync().GetAwaiter().GetResult();
        var window = TopLevel.GetTopLevel(content) as Window
            ?? throw new InvalidOperationException("The floating window was not created.");

        host.RestoreRequested += (_, _) => restoreRequested = true;
        host.CloseRequested += async (_, _) =>
        {
            closeRequested = true;
            await host.SetContentAsync(null);
            await host.CloseAsync();
            await host.DisposeAsync();
        };

        if (dock)
        {
            var panel = (DockPanel)window.Content!;
            var button = (Button)panel.Children[0];
            button.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        }
        else
        {
            window.Close();
        }
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

        Assert.Equal(dock, restoreRequested);
        Assert.Equal(!dock, closeRequested);
        Assert.Equal(dock, window.IsVisible);
        host.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
