using AsyncNavigation.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation;


internal abstract class PlatformServiceBase<TWindow> : IPlatformService<TWindow>
    where TWindow : class
{
    public abstract Task ShowAsync(TWindow window, bool isModal);
    public abstract void Show(TWindow window, bool isModal);

    public Task ShowAsync(IWindowBase baseWindow, bool isModal)
    {
        if(!TryGetPlatformWindow(baseWindow, out TWindow? window))
        {
            throw new InvalidOperationException($"Window must be of type {typeof(TWindow).Name}, " +
                $"but got {baseWindow?.GetType().Name ?? "null"}");
        }
        return ShowAsync(window, isModal);
    }
    public void Show(IWindowBase baseWindow, bool isModal)
    {
        if (!TryGetPlatformWindow(baseWindow, out TWindow? window))
        {
            throw new InvalidOperationException($"Window must be of type {typeof(TWindow).Name}, " +
                $"but got {window?.GetType().Name ?? "null"}");
        }
        Show(window, isModal);
    }
    
    private static bool TryGetPlatformWindow(IWindowBase baseWindow, [MaybeNullWhen(false)] out TWindow window)
    {
        return (window = baseWindow as TWindow) is not null;
    }

    public abstract T WaitOnDispatcher<T>(Task<T> task);

    public abstract void WaitOnDispatcher(Task task);
}
