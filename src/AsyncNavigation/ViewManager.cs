using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AsyncNavigation;

internal sealed class ViewManager : IViewManager, IViewPlacementCache
{
    private readonly ConcurrentDictionary<string, WeakReference<IView>> _viewCache = new();
    private readonly LinkedList<string> _lruList = [];
    private readonly Dictionary<string, LinkedListNode<string>> _lruIndex = [];
    private readonly object _lruLock = new();
    private readonly ViewCacheStrategy _strategy;
    private readonly int _maxCacheSize;
    private readonly IViewFactory _viewFactory;
    private readonly ViewPlacementCoordinator? _placement;

    public ViewManager(NavigationOptions options, IViewFactory viewFactory, ViewPlacementCoordinator? placement = null)
    {
        _placement = placement;
        _strategy = options.ViewCacheStrategy;
#pragma warning disable CS0618 // MaxCachedViews is obsolete but still used internally for view cache size
        _maxCacheSize = options.MaxCachedViews;
#pragma warning restore CS0618
        _viewFactory = viewFactory;
    }

    public void Clear()
    {
        foreach (var entry in _viewCache.ToArray())
        {
            if (entry.Value.TryGetTarget(out var view) && _placement?.IsFloating(view) == true)
                continue;
            Remove(entry.Key, dispose: true);
        }
    }
    public async Task<IView?> FindCachedViewAsync(string key, Func<IView, Task<bool>> isNavigationTarget)
    {
        if (_viewCache.TryGetValue(key, out var reference) &&
            reference.TryGetTarget(out var view) && await isNavigationTarget(view))
        {
            Touch(key);
            return view;
        }
        return null;
    }

    public async Task<IView> ResolveViewAsync(string key,
        bool useCache,
        Func<IView, Task<bool>>? isNavigationTarget = null,
        Func<IView, Task>? initialize = null)
    {
        if (useCache && _viewCache.TryGetValue(key, out var viewRef))
        {
            if (viewRef.TryGetTarget(out var view))
            {
                if (isNavigationTarget == null || await isNavigationTarget(view))
                {
                    Touch(key);
                    return view;
                }
            }
            else
            {
                Remove(key, dispose: false);
            }
        }

        var newView = _viewFactory.CreateView(key);

        try
        {
            if (initialize != null)
                await initialize(newView);

            AddView(key, newView);
            return newView;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            DisposeView(newView);
            throw;
        }
    }

    public void Remove(string cacheKey, bool dispose = false)
    {
        if (_viewCache.TryGetValue(cacheKey, out var pinned) &&
            pinned.TryGetTarget(out var active) && _placement?.IsFloating(active) == true) return;
        if (_viewCache.TryRemove(cacheKey, out var viewRef))
        {
            lock (_lruLock)
            {
                if (_lruIndex.TryGetValue(cacheKey, out var node))
                {
                    _lruList.Remove(node);
                    _lruIndex.Remove(cacheKey);
                }
            }

            if (dispose && viewRef.TryGetTarget(out var view))
            {
                DisposeView(view);
            }
        }
    }

    public void RemoveInstance(IView view)
    {
        foreach (var entry in _viewCache)
        {
            if (entry.Value.TryGetTarget(out var cached) && ReferenceEquals(cached, view))
                Remove(entry.Key);
        }
    }

    public void AddView(string cacheKey, IView view)
    {
        if (_strategy == ViewCacheStrategy.UpdateDuplicateKey)
        {
            _viewCache.AddOrUpdate(cacheKey, _ =>
            {
                AddToLru(cacheKey);
                return new WeakReference<IView>(view);
            }, (_, __) => new WeakReference<IView>(view));
        }
        else
        {
            if (_viewCache.TryAdd(cacheKey, new WeakReference<IView>(view)))
            {
                AddToLru(cacheKey);
            }
        }

        TrimCache(cacheKey);
    }

    private void AddToLru(string key)
    {
        lock (_lruLock)
        {
            if (_lruIndex.TryGetValue(key, out var existingNode))
            {
                _lruList.Remove(existingNode);
            }
            var node = _lruList.AddFirst(key);
            _lruIndex[key] = node;
        }
    }

    private void Touch(string key) => AddToLru(key);

    private void TrimCache(string protectedKey)
    {
        while (_viewCache.Count > _maxCacheSize)
        {
            string? oldestKey = null;
            lock (_lruLock)
            {
                var node = _lruList.Last;
                while (node is not null)
                {
                    if (node.Value != protectedKey &&
                        (!_viewCache.TryGetValue(node.Value, out var candidate) ||
                         !candidate.TryGetTarget(out var view) || _placement?.IsFloating(view) != true))
                        break;
                    node = node.Previous;
                }
                if (node is null) break; // Active windows are pinned, even above the cache limit.
                oldestKey = node.Value;
                _lruIndex.Remove(oldestKey);
                _lruList.Remove(node);
            }

            if (oldestKey != null && _viewCache.TryRemove(oldestKey, out var viewRef))
            {
                if (viewRef.TryGetTarget(out var view))
                {
                    DisposeView(view);
                }
            }
        }
    }

    public void Dispose()
    {
        Clear();
    }

    private void DisposeView(IView view)
    {
        if (_placement?.IsFloating(view) == true) return;
        SafeDispose(view, nameof(view));
        SafeDispose(view.DataContext, nameof(view.DataContext));
    }

    private static void SafeDispose(object? obj, string name)
    {
        if (obj is IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                Debug.Fail($"Dispose {name} error.", ex.ToString());
                throw;
            }
        }
    }
}
