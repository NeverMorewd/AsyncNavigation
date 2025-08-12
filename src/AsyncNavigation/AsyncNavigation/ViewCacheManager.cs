using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation;

internal class ViewCacheManager : IViewCacheManager
{
    private readonly ConcurrentDictionary<string, IView> _viewCache = new();
    private readonly ConcurrentQueue<string> _cacheKeys = new();

    public void ClearCache()
    {
        foreach (var view in _viewCache.Values)
        {
            DisposeView(view);
        }

        _viewCache.Clear();
        while (_cacheKeys.TryDequeue(out _)) { }
    }

    public Task<IView?> GetCachedViewAsync(string cacheKey)
    {
        _viewCache.TryGetValue(cacheKey, out var view);
        return Task.FromResult(view);
    }
    public bool TryCachedView(string cacheKey, [MaybeNullWhen(false)] out IView view)
    {
        return _viewCache.TryGetValue(cacheKey, out view);
    }
    public void RemoveFromCache(string cacheKey)
    {
        if (_viewCache.TryRemove(cacheKey, out var removedView))
        {
            DisposeView(removedView);
            var tempList = new List<string>();
            while (_cacheKeys.TryDequeue(out var k))
            {
                if (k != cacheKey)
                    tempList.Add(k);
            }
            foreach (var k in tempList)
                _cacheKeys.Enqueue(k);
        }
    }

    public Task SetCachedViewAsync(string cacheKey, IView view)
    {
        if (!_viewCache.ContainsKey(cacheKey))
        {
            _cacheKeys.Enqueue(cacheKey);
        }

        _viewCache[cacheKey] = view;

        while (_cacheKeys.Count > NavigationOptions.Default.MaxCachedItems)
        {
            if (_cacheKeys.TryDequeue(out var oldestKey))
            {
                if (_viewCache.TryRemove(oldestKey, out var removedView))
                {
                    DisposeView(removedView);
                }
            }
        }
        return Task.CompletedTask;
    }

    private static void DisposeView(IView view)
    {
        if (view is IDisposable disposable)
        {
            disposable.Dispose();
        }
        if (view.DataContext is IDisposable vmDisposable)
        {
            vmDisposable.Dispose();
        }
    }
}

