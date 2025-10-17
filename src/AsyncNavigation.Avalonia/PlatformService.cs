using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System.Diagnostics.CodeAnalysis;

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
    public override void AttachClosing(Window window, Action<object?, Core.WindowClosingEventArgs> handler)
    {
        window.Closing += (s, e) => 
        {
            var args = new Core.WindowClosingEventArgs { Cancel = e.Cancel };
            handler(s, args);
            e.Cancel = args.Cancel;
        };
    }
    public override void ShowMainWindow(Window mainWindow)
    {
        if (TryGetDesktopLifetime(out var lifetime))
        {
            lifetime.MainWindow = mainWindow;
            mainWindow.Show();
            return;
        }
        throw new NotSupportedException($"Lifetime: '{Application.Current!.ApplicationLifetime?.GetType()}' is not supported");
    }
    public void DetachClosing(Window window, Func<object?, Core.WindowClosingEventArgs, Task> handler)
    {
       
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
            if (!CheckLifetime())
            {
                if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
                {
                    desktopLifetime.MainWindow = window;
                }
            }
            window.Show();
        }
    }

    private static T WaitOnDispatcherFrame<T>(Task<T> task)
    {
        if (!task.IsCompleted)
        {
            Ensurelifetime();
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
            Ensurelifetime();
            var frame = new DispatcherFrame();
            task.ContinueWith(static (_, s) => ((DispatcherFrame)s!).Continue = false, frame);
            Dispatcher.UIThread.PushFrame(frame);
        }
        task.GetAwaiter().GetResult();
    }
    private static void Ensurelifetime()
    {
        if (!CheckLifetime())
        {
            throw new InvalidOperationException("lifetime has not been ready yet!");
        }
    }
    private static bool CheckLifetime()
    {
        var lifetimeReady = Application.Current != null && Application.Current!.ApplicationLifetime != null;
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopStyleApplicationLifetime)
        {
            lifetimeReady = lifetimeReady && desktopStyleApplicationLifetime.MainWindow != null;
        }
        return lifetimeReady;
    }
    private static bool TryGetDesktopLifetime([MaybeNullWhen(false)]out IClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime)
    {
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            classicDesktopStyleApplicationLifetime = lifetime;
            return true;
        }
        classicDesktopStyleApplicationLifetime = null;
        return false;
    }
}
