using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation;


internal abstract class PlatformServiceBase<TWindow> : IPlatformService<TWindow>
    where TWindow : class
{
    private readonly Dictionary<Action<object?, WindowClosingEventArgs>, Action> _detachers = [];

    public abstract Task ShowAsync(TWindow window, bool isModal);
    public abstract void Show(TWindow window, bool isModal);
    public abstract Action AttachClosingCore(TWindow window, Action<object?, WindowClosingEventArgs> handler);
    public abstract void ShowMainWindow(TWindow mainWindow);
    public Task ShowAsync(IDialogWindowBase baseWindow, bool isModal)
    {
        if(!TryGetPlatformWindow(baseWindow, out TWindow? window))
        {
            throw new InvalidOperationException($"Window must be of type {typeof(TWindow).Name}, " +
                $"but got {baseWindow?.GetType().Name ?? "null"}");
        }
        return ShowAsync(window, isModal);
    }
    public void Show(IDialogWindowBase baseWindow, bool isModal)
    {
        if (!TryGetPlatformWindow(baseWindow, out TWindow? window))
        {
            throw new InvalidOperationException($"Window must be of type {typeof(TWindow).Name}, " +
                $"but got {baseWindow?.GetType().Name ?? "null"}");
        }
        Show(window, isModal);
    }
    public abstract T WaitOnDispatcher<T>(Task<T> task);

    public abstract void WaitOnDispatcher(Task task);

    public void AttachClosing(TWindow window, Action<object?, WindowClosingEventArgs> handler)
    {
        var detacher = AttachClosingCore(window, handler);
        lock (_detachers)
            _detachers[handler] = detacher;
    }

    public void AttachClosing(IDialogWindowBase baseWindow, Action<object?, WindowClosingEventArgs> handler)
    {
        if (!TryGetPlatformWindow(baseWindow, out TWindow? window))
        {
            throw new InvalidOperationException($"Window must be of type {typeof(TWindow).Name}, " +
                $"but got {baseWindow?.GetType().Name ?? "null"}");
        }
        AttachClosing(window, handler);
    }

    public void DetachClosing(IDialogWindowBase baseWindow, Action<object?, WindowClosingEventArgs> handler)
    {
        Action? detacher;
        lock (_detachers)
        {
            if (!_detachers.TryGetValue(handler, out detacher))
                return;
            _detachers.Remove(handler);
        }
        detacher?.Invoke();
    }


    public void ShowMainWindow(object mainWindow)
    {
        if (!TryGetPlatformWindow(mainWindow, out TWindow? window))
        {
            throw new InvalidOperationException($"Window must be of type {typeof(TWindow).Name}, " +
                $"but got {mainWindow?.GetType().Name ?? "null"}");
        }
        ShowMainWindow(window);
    }
    private static bool TryGetPlatformWindow(object baseWindow, [MaybeNullWhen(false)] out TWindow window)
    {
        return (window = baseWindow as TWindow) is not null;
    }
}
