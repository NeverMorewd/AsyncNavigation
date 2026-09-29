using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Runtime.CompilerServices;

namespace AsyncNavigation;

/// <summary>
/// Observes unload requests on an <see cref="INavigationAware"/> using a weak reference,
/// ensuring callbacks are invoked without preventing garbage collection.
/// </summary>
internal sealed class WeakUnloadObserver
{
    // Tracks the currently-subscribed handler per instance so re-subscribing the same
    // INavigationAware (e.g. after floating and docking a view back) replaces the previous
    // handler instead of stacking a second one that would fire alongside it.
    private static readonly ConditionalWeakTable<INavigationAware, AsyncEventHandler<AsyncEventArgs>> _subscriptions = new();

    public static void Subscribe(INavigationAware navigationAware, Action<INavigationAware> onUnloadCallback)
    {
        if (_subscriptions.TryGetValue(navigationAware, out var previousHandler))
        {
            navigationAware.AsyncRequestUnloadEvent -= previousHandler;
            _subscriptions.Remove(navigationAware);
        }

        var weakReference = new WeakReference<INavigationAware>(navigationAware);

        async Task HandleRequestUnloadAsync(object? sender, AsyncEventArgs args)
        {
            if (!weakReference.TryGetTarget(out var target))
            {
                if (sender is INavigationAware aware)
                {
                    aware.AsyncRequestUnloadEvent -= HandleRequestUnloadAsync;
                }
                return;
            }

            await target.OnUnloadAsync(args.CancellationToken);
            onUnloadCallback?.Invoke(target);
        }

        _subscriptions.Add(navigationAware, HandleRequestUnloadAsync);
        navigationAware.AsyncRequestUnloadEvent += HandleRequestUnloadAsync;
    }
}
