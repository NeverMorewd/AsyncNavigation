using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation;

/// <summary>
/// Observes unload requests on an <see cref="INavigationAware"/> using a weak reference,
/// ensuring callbacks are invoked without preventing garbage collection.
/// </summary>
internal sealed class WeakUnloadObserver
{
    public static void Subscribe(INavigationAware navigationAware, Action<INavigationAware> onUnloadCallback)
    {
        var weakReference = new WeakReference<INavigationAware>(navigationAware);

        async Task HandleRequestUnloadAsync(object? sender, AsyncEventArgs args)
        {
            if (!weakReference.TryGetTarget(out var target))
            {
                if (sender is INavigationAware aware)
                {
                    aware.RequestUnloadAsync -= HandleRequestUnloadAsync;
                }
                return;
            }

            await target.OnUnloadAsync(args.CancellationToken);
            onUnloadCallback?.Invoke(target);
        }

        navigationAware.RequestUnloadAsync += HandleRequestUnloadAsync;
    }
}

