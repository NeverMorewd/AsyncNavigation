// File: RegionManagerBase.cs
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation;

public abstract class RegionManagerBase : IRegionManager, IDisposable
{
    private static readonly object _staticLock = new();
    private static RegionManagerBase? _current;
    private static readonly ConcurrentDictionary<string, (WeakReference<object> Target, IServiceProvider? ServiceProvider, bool? PreferCache)> _tempRegionCache = [];

    private readonly object _regionLock = new();
    private readonly ConcurrentDictionary<string, WeakReference<IRegion>> _regions = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<(Func<Task<NavigationResult>> Task, TaskCompletionSource<NavigationResult> Tcs)>> _pendingNavigations = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IRegionFactory _regionFactory;
    private readonly int _maxReplayCount;

    private IRegion? _currentRegion;

    protected RegionManagerBase(IRegionFactory regionFactory, IServiceProvider serviceProvider)
    {
        lock (_staticLock)
        {
            if (_current is not null)
                throw new InvalidOperationException("RegionManager is already created. Only one instance is allowed.");
            _current = this;
        }

        _regionFactory = regionFactory;
        _serviceProvider = serviceProvider;
        _maxReplayCount = serviceProvider.GetRequiredService<NavigationOptions>().MaxReplayItems;

        RecoverTempRegions();
    }

    public IReadOnlyDictionary<string, IRegion> Regions
    {
        get
        {
            CleanupCollectedRegions();
            return _regions
                .Where(kv => kv.Value.TryGetTarget(out _))
                .ToDictionary(kv => kv.Key, kv => kv.Value.TryGetTarget(out var r) ? r! : null!)
                .Where(kv => kv.Value is not null)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

    public async Task<NavigationResult> RequestNavigateAsync(
        string regionName,
        string viewName,
        INavigationParameters? navigationParameters = null,
        bool replay = false,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetRegion(regionName, out var region))
        {
            if (replay)
                return await EnqueueReplayNavigation(regionName, viewName, navigationParameters, cancellationToken);

            throw new InvalidOperationException($"Region '{regionName}' not found or has been collected.");
        }

        return await PerformNavigationAsync(region, regionName, viewName, navigationParameters, cancellationToken);
    }

    private async Task<NavigationResult> PerformNavigationAsync(
        IRegion region,
        string regionName,
        string viewName,
        INavigationParameters? parameters,
        CancellationToken cancellationToken)
    {
        var context = new NavigationContext
        {
            RegionName = regionName,
            ViewName = viewName,
            Parameters = parameters
        };
        context.LinkCancellationToken(cancellationToken);

        if (context.CancellationToken.IsCancellationRequested)
            return NavigationResult.Cancelled(context);

        try
        {
            IRegion? previousRegion;
            lock (_regionLock)
            {
                previousRegion = _currentRegion;
                _currentRegion = region;
            }

            if (previousRegion is not null && previousRegion != region)
                await previousRegion.NavigateFromAsync(context);

            await region.ActivateViewAsync(context);
            Debug.WriteLine($"[Navigate] → {viewName} @ {regionName}");
            return NavigationResult.Success(context);
        }
        catch (Exception ex)
        {
            return await HandleNavigationException(ex, region, context);
        }
    }

    private async Task<NavigationResult> HandleNavigationException(Exception ex, IRegion region, NavigationContext context)
    {
        if (ex is OperationCanceledException)
        {
            Debug.WriteLine($"[Cancel] {context}");
            await region.RevertAsync(context);
            return NavigationResult.Cancelled(context);
        }

        Debug.WriteLine($"[Error] {context} -> {ex}");
        return NavigationResult.Failure(ex, context);
    }

    private async Task<NavigationResult> EnqueueReplayNavigation(
        string regionName,
        string viewName,
        INavigationParameters? parameters,
        CancellationToken token)
    {
        var queue = _pendingNavigations.GetOrAdd(regionName, _ => new());
        var tcs = new TaskCompletionSource<NavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        queue.Enqueue((() => RequestNavigateAsync(regionName, viewName, parameters, false, token), tcs));
        while (queue.Count > _maxReplayCount && queue.TryDequeue(out _)) { }

        Debug.WriteLine($"[Replay] Region '{regionName}' not found, request cached.");
        return await tcs.Task;
    }

    public void AddRegion(string regionName, IRegion region)
    {
        if (!_regions.TryAdd(regionName, new WeakReference<IRegion>(region)))
            throw new InvalidOperationException($"Duplicated RegionName found: {regionName}");

        _ = TryReplayPendingNavigations(regionName);
    }

    public bool TryGetRegion(string name, [MaybeNullWhen(false)] out IRegion region)
    {
        region = null;
        if (_regions.TryGetValue(name, out var weakRef) && weakRef.TryGetTarget(out var target))
        {
            region = target;
            return true;
        }
        _regions.TryRemove(name, out _);
        return false;
    }

    public bool TryRemoveRegion(string name, [MaybeNullWhen(false)] out IRegion region)
    {
        region = null;
        if (_regions.TryRemove(name, out var weakRef) && weakRef.TryGetTarget(out var target))
        {
            region = target;
            return true;
        }
        return false;
    }

    protected void CreateRegionSafe(string name, object target, IServiceProvider? provider, bool? preferCache)
    {
        lock (_regionLock)
        {
            if (TryGetRegion(name, out _))
                throw new InvalidOperationException($"Duplicated RegionName found: {name}");
            provider ??= _serviceProvider;
            var region = _regionFactory.CreateRegion(name, target, provider, preferCache);
            AddRegion(name, region);
        }
    }

    private async Task TryReplayPendingNavigations(string name)
    {
        if (_pendingNavigations.TryRemove(name, out var queue))
        {
            Debug.WriteLine($"[Replay] Found {queue.Count} cached navigations for '{name}', replaying...");
            while (queue.TryDequeue(out var item))
            {
                var (taskFactory, tcs) = item;
                var result = await taskFactory();
                tcs.TrySetResult(result);
            }
        }
    }

    private void CleanupCollectedRegions()
    {
        foreach (var key in _regions.Keys)
        {
            if (_regions.TryGetValue(key, out var weakRef) && !weakRef.TryGetTarget(out _))
                _regions.TryRemove(key, out _);
        }
    }

    private void RecoverTempRegions()
    {
        lock (_staticLock)
        {
            foreach (var kv in _tempRegionCache)
            {
                if (kv.Value.Target.TryGetTarget(out var target) && target is not null)
                    CreateRegionSafe(kv.Key, target, kv.Value.ServiceProvider, kv.Value.PreferCache);
            }
            _tempRegionCache.Clear();
        }
    }

    protected static void OnAddRegionNameCore(string name, object d, IServiceProvider? sp, bool? preferCache)
    {
        lock (_staticLock)
        {
            if (_current == null)
            {
                if (!_tempRegionCache.TryAdd(name, (new WeakReference<object>(d), sp, preferCache)))
                    throw new InvalidOperationException($"Duplicated RegionName found: {name}");
            }
            else
            {
                _current.CreateRegionSafe(name, d, sp, preferCache);
            }
        }
    }

    protected static void OnRemoveRegionNameCore(string name)
    {
        lock (_staticLock)
        {
            _current?.TryRemoveRegion(name, out _);
        }
    }

    // --- Implement remaining IRegionManager navigation methods ---

    public async Task<NavigationResult> GoForward(string regionName, CancellationToken cancellationToken = default)
    {
        var region = GetRegion(regionName);
        if (await region.CanGoForwardAsync())
        {
            try
            {
                await region.GoForwardAsync(cancellationToken);
                Debug.WriteLine($"[GoForward] {regionName}");
                return NavigationResult.Success(TimeSpan.Zero);
            }
            catch (OperationCanceledException)
            {
                await region.RevertAsync(null);
                return NavigationResult.Cancelled();
            }
        }
        return NavigationResult.Failure(new NavigationException("Cannot go forward."));
    }

    public async Task<NavigationResult> GoBack(string regionName, CancellationToken cancellationToken = default)
    {
        var region = GetRegion(regionName);
        if (await region.CanGoBackAsync())
        {
            try
            {
                await region.GoBackAsync(cancellationToken);
                Debug.WriteLine($"[GoBack] {regionName}");
                return NavigationResult.Success(TimeSpan.Zero);
            }
            catch (OperationCanceledException)
            {
                await region.RevertAsync(null);
                return NavigationResult.Cancelled();
            }
        }
        return NavigationResult.Failure(new NavigationException("Cannot go back."));
    }

    public Task<bool> CanGoForwardAsync(string regionName)
        => GetRegion(regionName).CanGoForwardAsync();

    public Task<bool> CanGoBackAsync(string regionName)
        => GetRegion(regionName).CanGoBackAsync();

    protected IRegion GetRegion(string regionName)
    {
        if (TryGetRegion(regionName, out var region))
            return region!;
        throw new InvalidOperationException($"Region '{regionName}' not found or has been collected.");
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var kv in _regions)
            if (kv.Value.TryGetTarget(out var region))
                region.Dispose();

        _regions.Clear();
        _pendingNavigations.Clear();

        lock (_staticLock)
            _tempRegionCache.Clear();

        Debug.WriteLine("[Dispose] RegionManager disposed.");
    }
}
