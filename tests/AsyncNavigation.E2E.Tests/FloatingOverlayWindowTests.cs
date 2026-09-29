using AsyncNavigation.Avalonia.Floating;
using AsyncNavigation.Floating;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AsyncNavigation.E2E.Tests;

public sealed class FloatingOverlayWindowTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Overlay_host_renders_in_page_and_raises_separate_requests(bool dock)
    {
        var mainView = new Grid();
        var window = new Window { Content = mainView, Width = 500, Height = 400 };
        window.Show();
        try
        {
            var content = new Border();
            var restoreRequested = false;
            var closeRequested = false;
            var host = new AvaloniaOverlayFloatingWindowHost(
                new FloatingWindowOptions { Title = "Overlay lifecycle test", Width = 320, Height = 240 },
                mainView);

            host.RestoreRequested += (_, _) => restoreRequested = true;
            host.CloseRequested += (_, _) => closeRequested = true;

            host.SetContentAsync(content).GetAwaiter().GetResult();
            host.ShowAsync().GetAwaiter().GetResult();
            Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

            var topLevel = TopLevel.GetTopLevel(mainView);
            var overlay = OverlayLayer.GetOverlayLayer(topLevel!);
            Assert.NotNull(overlay);
            // The overlay host must NOT open a separate OS window: floated content stays under
            // the same TopLevel as the page that hosts it - that is the whole point on WASM.
            Assert.Same(topLevel, TopLevel.GetTopLevel(content));

            var dockButton = FindButton(overlay!, "Dock");
            var closeButton = FindButton(overlay!, "X");

            if (dock)
                dockButton.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            else
                closeButton.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

            Dispatcher.UIThread.RunJobs(DispatcherPriority.Loaded);

            Assert.Equal(dock, restoreRequested);
            Assert.Equal(!dock, closeRequested);

            host.CloseAsync().GetAwaiter().GetResult();
            Assert.DoesNotContain(FindButtons(overlay!), b => b == dockButton);
            host.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        finally
        {
            window.Close();
        }
    }

    private static Button FindButton(OverlayLayer overlay, string text) =>
        FindButtons(overlay).First(b => (b.Content as string) == text);

    private static IEnumerable<Button> FindButtons(OverlayLayer overlay) =>
        overlay.GetVisualDescendants().OfType<Button>();
}
