using AsyncNavigation.Floating;
using AsyncNavigation.Wpf.Floating;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AsyncNavigation.Wpf.E2E.Tests;

public sealed class FloatingWindowLifecycleTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dock_and_close_raise_separate_requests(bool dock)
    {
        RunOnStaThread(() =>
        {
            var content = new Border();
            var restoreRequested = false;
            var closeRequested = false;
            var host = new WpfFloatingWindowHost(new FloatingWindowOptions
            {
                Title = "Floating lifecycle test",
                Width = 320,
                Height = 240
            });

            host.SetContentAsync(content).GetAwaiter().GetResult();
            host.ShowAsync().GetAwaiter().GetResult();
            var window = Window.GetWindow(content)
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
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else
            {
                window.Close();
            }
            FlushDispatcher();

            Assert.Equal(dock, restoreRequested);
            Assert.Equal(!dock, closeRequested);
            Assert.Equal(dock, window.IsVisible);
            host.DisposeAsync().AsTask().GetAwaiter().GetResult();
        });
    }

    private static void FlushDispatcher() =>
        Dispatcher.CurrentDispatcher.Invoke(static () => { }, DispatcherPriority.ApplicationIdle);

    private static void RunOnStaThread(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
