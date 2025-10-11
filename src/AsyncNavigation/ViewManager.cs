using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AsyncNavigation;

internal sealed class ViewManager : IViewManager
{
    private readonly ConcurrentDictionary<string, WeakReference<IView>> _viewCache = new();
    private readonly LinkedList<string> _lruList = new();
    private readonly object _lruLock = new();
    private readonly ViewCacheStrategy _strategy;
    private readonly int _maxCacheSize;
    private readonly IViewFactory _viewFactory;

    public ViewManager(NavigationOptions options, IViewFactory viewFactory)
    {
        _strategy = options.ViewCacheStrategy;
        _maxCacheSize = options.MaxCachedViews;
        _viewFactory = viewFactory;
    }

    public void Clear()
    {
        var values = _viewCache.Values.ToArray();
        _viewCache.Clear();
        lock (_lruLock)
        {
            _lruList.Clear();
        }

        foreach (var viewRef in values)
        {
            if (viewRef.TryGetTarget(out var view))
            {
                DisposeView(view);
            }
        }
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
        if (_viewCache.TryRemove(cacheKey, out var viewRef))
        {
            lock (_lruLock)
            {
                _lruList.Remove(cacheKey);
            }

            if (dispose && viewRef.TryGetTarget(out var view))
            {
                DisposeView(view);
            }
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

        TrimCache();
    }

    private void AddToLru(string key)
    {
        lock (_lruLock)
        {
            _lruList.Remove(key);
            _lruList.AddFirst(key);
        }
    }

    private void Touch(string key) => AddToLru(key);

    private void TrimCache()
    {
        while (_viewCache.Count > _maxCacheSize)
        {
            string? oldestKey = null;
            lock (_lruLock)
            {
                if (_lruList.Last != null)
                {
                    oldestKey = _lruList.Last.Value;
                    _lruList.RemoveLast();
                }
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

    private static void DisposeView(IView view)
    {
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
