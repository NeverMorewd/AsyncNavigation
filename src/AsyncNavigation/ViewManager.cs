using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AsyncNavigation;

internal sealed class ViewManager : IViewManager
{
    private readonly ConcurrentDictionary<string, WeakReference<IView>> _viewCache = new();
    private readonly ConcurrentQueue<string> _cacheKeys = new();
    private readonly ViewCacheStrategy _strategy;
    private readonly int _maxCacheSize;
    private readonly IViewFactory _viewFactory;
    private int _cacheCount;

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
        while (_cacheKeys.TryDequeue(out _)) { }

        Interlocked.Exchange(ref _cacheCount, 0);

        foreach (var viewRef in values)
        {
            if (viewRef.TryGetTarget(out var view))
            {
                DisposeView(view);
            }
        }
    }

    public async Task<IView> ResolveViewAsync(string key, bool useCache, NavigationContext navigationContext)
    {
        if (useCache && _viewCache.TryGetValue(key, out var viewRef))
        {
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
            if (viewRef.TryGetTarget(out var view) && view.DataContext is INavigationAware navigationAware)
            {
                if (await navigationAware.IsNavigationTargetAsync(navigationContext))
                {
                    navigationContext.CancellationToken.ThrowIfCancellationRequested();
                    return view;
                }
            }
        }
        var newView = _viewFactory.CreateView(key);
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        if (newView.DataContext is INavigationAware aware)
        {
            await aware.InitializeAsync(navigationContext.CancellationToken);
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
        }
        AddView(key, newView);
        return newView;
    }

    public void Remove(string cacheKey, bool dispose = false)
    {
        if (_viewCache.TryRemove(cacheKey, out var viewRef))
        {
            if (dispose && viewRef.TryGetTarget(out var view))
            {
                DisposeView(view);
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
                return new WeakReference<IView>(view);
            }, (_, __) => new WeakReference<IView>(view));
        }
        else
        {
            if (_viewCache.TryAdd(cacheKey, new WeakReference<IView>(view)))
            {
                _cacheKeys.Enqueue(cacheKey);
                Interlocked.Increment(ref _cacheCount);
            }
        }

        while (Volatile.Read(ref _cacheCount) > _maxCacheSize)
        {
            if (_cacheKeys.TryDequeue(out var oldestKey))
            {
                if (_viewCache.TryRemove(oldestKey, out var viewRef))
                {
                    Interlocked.Decrement(ref _cacheCount);
                    if (viewRef.TryGetTarget(out var viewToDispose))
                    {
                        DisposeView(viewToDispose);
                    }
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
