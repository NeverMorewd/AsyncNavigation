using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AsyncNavigation;

internal sealed class ViewManager : IViewManager
{
    private readonly ConcurrentDictionary<string, IView> _viewCache = new();
    private readonly ConcurrentQueue<string> _cacheKeys = new();
    private readonly ViewCacheStrategy _strategy;
    private readonly int _maxCacheSize;
    private readonly IViewFactory _viewFactory;
    private int _cacheCount;

    public ViewManager(NavigationOptions options, IViewFactory viewFactory)
    {
        _strategy = options.ViewCacheStrategy;
        _maxCacheSize = options.MaxCachedItems;
        _viewFactory = viewFactory;
    }

    public void Clear()
    {
        var values = _viewCache.Values.ToArray();
        _viewCache.Clear();
        while (_cacheKeys.TryDequeue(out _)) { }

        Interlocked.Exchange(ref _cacheCount, 0);

        foreach (var view in values)
        {
            DisposeView(view);
        }
    }

    public async Task<IView> ResolveViewAsync(string key, bool useCache, NavigationContext navigationContext)
    {
        if (useCache && _viewCache.TryGetValue(key, out var view))
        {
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
            if (view.DataContext is INavigationAware navigationAware)
            {
                if (await navigationAware.IsNavigationTargetAsync(navigationContext))
                {
                    navigationContext.CancellationToken.ThrowIfCancellationRequested();
                    return view;
                }
            }
        }
        view = _viewFactory.CreateView(key);
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        if (view.DataContext is INavigationAware aware)
        {
            await aware.InitializeAsync(navigationContext.CancellationToken);
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
        }
        AddView(key, view);
        return view;
    }

    public void Remove(string cacheKey, bool dispose = false)
    {
        if (_viewCache.TryRemove(cacheKey, out var removedView))
        {
            if (dispose)
            {
                DisposeView(removedView);
            }
            Interlocked.Decrement(ref _cacheCount);
        }
    }

    public void AddView(string cacheKey, IView view)
    {
        if (_strategy == ViewCacheStrategy.UpdateDuplicateKey)
        {
            _viewCache.AddOrUpdate(cacheKey, _ =>
            {
                _cacheKeys.Enqueue(cacheKey);
                Interlocked.Increment(ref _cacheCount);
                return view;
            }, (_, __) => view);
        }
        else
        {
            if (_viewCache.TryAdd(cacheKey, view))
            {
                _cacheKeys.Enqueue(cacheKey);
                Interlocked.Increment(ref _cacheCount);
            }
        }

        while (Volatile.Read(ref _cacheCount) > _maxCacheSize)
        {
            if (_cacheKeys.TryDequeue(out var oldestKey))
            {
                if (_viewCache.TryRemove(oldestKey, out var removedView))
                {
                    Interlocked.Decrement(ref _cacheCount);
                    DisposeView(removedView);
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
#if DEBUG
        GcMonitor.Attach(view);
#endif
        SafeDispose(view, nameof(view));

#if DEBUG
        if (view.DataContext != null)
            GcMonitor.Attach(view.DataContext);
#endif
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
                NavigationDiagnostics.Report(ex, $"Dispose {name} error:");
            }
        }
    }
}
