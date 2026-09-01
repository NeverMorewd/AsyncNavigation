using AsyncNavigation.Floating;
using AsyncNavigation.Wpf.Floating;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AsyncNavigation.Wpf.E2E.Tests;

public sealed class FloatingWindowLifecycleTests
{
    [Fact]
    public void User_close_restores_content_and_then_closes_the_window()
    {
        RunOnStaThread(() =>
        {
            var content = new Border();
            var restoredContent = new ContentControl();
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

            host.RestoreRequested += async (_, _) =>
            {
                await host.SetContentAsync(null);
                restoredContent.Content = content;
                await host.CloseAsync();
                await host.DisposeAsync();
            };

            window.Close();
            FlushDispatcher();

            Assert.Same(content, restoredContent.Content);
            Assert.False(window.IsVisible);
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
