using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Windows;
using System.Windows.Threading;

namespace AsyncNavigation.Wpf;

internal class PlatformService : PlatformServiceBase<Window>
{
    /// <inheritdoc/>
    /// <remarks>
    /// In WPF every dialog window is a <see cref="Window"/>, which carries its own
    /// <see cref="System.Windows.Threading.Dispatcher"/> and is the natural host for
    /// platform-level operations.  The context is ready as soon as the Window object
    /// is constructed — before it is shown on screen.
    /// </remarks>
    public override IViewContext CreateDialogViewContext(IDialogWindowBase dialogWindow)
    {
        if (dialogWindow is Window window)
            return new ViewContext(window);
        throw new InvalidOperationException(
            $"Dialog window must be a WPF Window, but got {dialogWindow?.GetType().Name ?? "null"}.");
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

        if (isModal)
        {
            window.ShowDialog();
        }
        else
        {
            window.Show();
        }
    }

    public override Task ShowAsync(Window window, bool isModal)
    {
        Show(window, isModal);
        return Task.CompletedTask;
    }

    private static T WaitOnDispatcherFrame<T>(Task<T> task)
    {
        if (!task.IsCompleted)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(static (_, s) => ((DispatcherFrame)s!).Continue = false, frame);
            Dispatcher.PushFrame(frame);
        }
        return task.GetAwaiter().GetResult();
    }

    private static void WaitOnDispatcherFrame(Task task)
    {
        if (!task.IsCompleted)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(static (_, s) => ((DispatcherFrame)s!).Continue = false, frame);
            Dispatcher.PushFrame(frame);
        }
        task.GetAwaiter().GetResult();
    }

    public override void AttachClosing(Window window, Action<object?, WindowClosingEventArgs> handler)
    {
        window.Closing += (s, e) => 
        {
            var args = new WindowClosingEventArgs { Cancel = e.Cancel };
            handler(s, args);
            e.Cancel = args.Cancel;
        };
    }

    public override void ShowMainWindow(Window mainWindow)
    {
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
    }
}
