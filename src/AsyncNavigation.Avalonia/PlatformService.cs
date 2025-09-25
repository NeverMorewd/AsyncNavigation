using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace AsyncNavigation.Avalonia;

internal class PlatformService : PlatformServiceBase<Window>
{
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
            Window? owner = null;
            if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                owner = desktopLifetime.Windows.LastOrDefault(w => w.IsActive);
                owner ??= desktopLifetime.MainWindow;
            }
            else
            {
                throw new NotSupportedException($"Lifetime: '{Application.Current!.ApplicationLifetime?.GetType()}' is not supported");
            }
            if (owner != null)
            {
                var showTask = window.ShowDialog(owner);
                WaitOnDispatcherFrame(showTask);
            }
        }
        else
        {
            window.Show();
        }
    }

    public override async Task ShowAsync(Window window, bool isModal)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (isModal)
        {
            Window? owner = null;
            if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                owner = desktopLifetime.Windows.LastOrDefault(w => w.IsActive);
                owner ??= desktopLifetime.MainWindow;
            }
            else
            {
                throw new NotSupportedException($"Lifetime: '{Application.Current!.ApplicationLifetime?.GetType()}' is not supported");
            }
            if (owner != null)
            {
                await window.ShowDialog(owner);
            }
        }
        else
        {
            window.Show();
        }
    }

    private static T WaitOnDispatcherFrame<T>(Task<T> task)
    {
        if (!task.IsCompleted)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(static (_, s) => ((DispatcherFrame)s!).Continue = false, frame);
            Dispatcher.UIThread.PushFrame(frame);
        }
        return task.GetAwaiter().GetResult();
    }

    private static void WaitOnDispatcherFrame(Task task)
    {
        if (!task.IsCompleted)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(static (_, s) => ((DispatcherFrame)s!).Continue = false, frame);
            Dispatcher.UIThread.PushFrame(frame);
        }
        task.GetAwaiter().GetResult();
    }
}
