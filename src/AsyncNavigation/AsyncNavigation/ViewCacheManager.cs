using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation;

internal class ViewCacheManager : IViewCacheManager
{
    private readonly ConcurrentDictionary<string, IView> _viewCache = new();
    private readonly ConcurrentQueue<string> _cacheKeys = new();
    private readonly ViewCacheStrategy _strategy;
    private readonly int _maxCacheSize;
    public ViewCacheManager(NavigationOptions options)
    {
        _strategy = options.ViewCacheStrategy;
        _maxCacheSize = options.MaxCachedItems;
    }

    public void Clear()
    {
        foreach (var view in _viewCache.Values)
        {
            DisposeView(view);
        }

        _viewCache.Clear();
        while (_cacheKeys.TryDequeue(out _)) { }
    }

    public Task<IView?> GetView(string cacheKey)
    {
        _viewCache.TryGetValue(cacheKey, out var view);
        return Task.FromResult(view);
    }
    public bool TryAddView(string cacheKey, [MaybeNullWhen(false)] out IView view)
    {
        return _viewCache.TryGetValue(cacheKey, out view);
    }
    public void Remove(string cacheKey)
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

    public Task SetView(string cacheKey, IView view)
    {
        if (_viewCache.ContainsKey(cacheKey))
        {
            if (_strategy == ViewCacheStrategy.UpdateDuplicateKey)
            {
                _viewCache[cacheKey] = view;
            }
        }
        else
        {
            _cacheKeys.Enqueue(cacheKey);
            _viewCache[cacheKey] = view;
        }

        while (_cacheKeys.Count > _maxCacheSize)
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

