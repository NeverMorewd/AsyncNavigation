using AsyncNavigation.Avalonia.Floating;
using AsyncNavigation.Floating;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;

namespace AsyncNavigation.E2E.Tests;

public sealed class FloatingWindowLifecycleTests
{
    [AvaloniaFact]
    public void User_close_restores_content_and_then_closes_the_window()
    {
        var content = new Border();
        var restoredContent = new ContentControl();
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

        host.RestoreRequested += async (_, _) =>
        {
            await host.SetContentAsync(null);
            restoredContent.Content = content;
            await host.CloseAsync();
            await host.DisposeAsync();
        };

        window.Close();
        Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

        Assert.Same(content, restoredContent.Content);
        Assert.False(window.IsVisible);
    }
}
