using AsyncNavigation.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Windows.Foundation;
using System;
using System.Threading.Tasks;

namespace AsyncNavigation.WinUI;

internal class PlatformService : PlatformServiceBase<Window>
{
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue =
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
        ?? throw new InvalidOperationException("AddNavigationSupport must be called on the WinUI UI thread.");

    public void PostToUIThread(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        // Always enqueue, even when already on the UI thread. Closing a WinUI
        // Window from inside AppWindow.Closing is re-entrant: the outer handler
        // subsequently applies Cancel=true and prevents the nested Close call.
        if (!_dispatcherQueue.TryEnqueue(() => action()))
            throw new InvalidOperationException("The WinUI dispatcher queue is shutting down.");
    }

    public override T WaitOnDispatcher<T>(Task<T> task)
    {
        return WaitOnDispatcherFrame(task);
    }

    public override void WaitOnDispatcher(Task task)
    {
        WaitOnDispatcherFrame(task);
    }
    public override void Show(Window window, bool isModal)
    {
        ArgumentNullException.ThrowIfNull(window);

        window.Activate();
    }

    public override Task ShowAsync(Window window, bool isModal)
    {
        Show(window, isModal);
        return Task.CompletedTask;
    }

    private static T WaitOnDispatcherFrame<T>(Task<T> task)
    {
        return task.GetAwaiter().GetResult();
    }

    private static void WaitOnDispatcherFrame(Task task)
    {
        task.GetAwaiter().GetResult();
    }

    public override Action AttachClosingCore(Window window, Action<object?, WindowClosingEventArgs> handler)
    {
        TypedEventHandler<AppWindow, AppWindowClosingEventArgs> wrapper = (s, e) =>
        {
            var args = new WindowClosingEventArgs { Cancel = e.Cancel };
            handler(s, args);
            e.Cancel = args.Cancel;
        };
        window.AppWindow.Closing += wrapper;
        return () => window.AppWindow.Closing -= wrapper;
    }

    public override void ShowMainWindow(Window mainWindow)
    {
        mainWindow.Activate();
    }
}
