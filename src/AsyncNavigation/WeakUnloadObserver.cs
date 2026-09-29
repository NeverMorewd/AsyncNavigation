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
    private static readonly object _subscriptionsLock = new();

    public static void Subscribe(INavigationAware navigationAware, Action<INavigationAware> onUnloadCallback)
    {
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

        // TryGetValue/Remove/Add aren't individually atomic against each other, and
        // ConditionalWeakTable.Add throws on a duplicate key - serialize the read-modify-write
        // so two concurrent Subscribe calls for the same instance can't both pass the
        // TryGetValue check and race on Add.
        AsyncEventHandler<AsyncEventArgs>? previousHandler;
        lock (_subscriptionsLock)
        {
            _subscriptions.TryGetValue(navigationAware, out previousHandler);
            if (previousHandler is not null)
                _subscriptions.Remove(navigationAware);
            _subscriptions.Add(navigationAware, HandleRequestUnloadAsync);
        }
        if (previousHandler is not null)
            navigationAware.AsyncRequestUnloadEvent -= previousHandler;
        navigationAware.AsyncRequestUnloadEvent += HandleRequestUnloadAsync;
    }
}
