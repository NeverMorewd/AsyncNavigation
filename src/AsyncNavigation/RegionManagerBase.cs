using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation;

public abstract class RegionManagerBase : IRegionManager, IDisposable
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IRegionFactory _regionFactory;
    protected readonly ConcurrentDictionary<string, WeakReference<IRegion>> _regions;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<(Func<Task<NavigationResult>> Task, TaskCompletionSource<NavigationResult> Tcs)>> _pendingNavigations;
    private IRegion? _currentRegion;
    private readonly int _maxReplayCount;
    private static readonly ConcurrentDictionary<string, (WeakReference<object> Target, IServiceProvider? ServiceProvider, bool? PreferCache)> _tempRegionCache = [];
    private static RegionManagerBase? _current;

    protected RegionManagerBase(IRegionFactory regionFactory, IServiceProvider serviceProvider)
    {
        if (Volatile.Read(ref _current) != null)
            throw new InvalidOperationException("RegionManager is already created. Only one instance is allowed.");
        
        _regions = new ConcurrentDictionary<string, WeakReference<IRegion>>();
        _pendingNavigations = new ConcurrentDictionary<string, ConcurrentQueue<(Func<Task<NavigationResult>> Task, TaskCompletionSource<NavigationResult> Tcs)>>();
        _serviceProvider = serviceProvider;
        _regionFactory = regionFactory;
        _maxReplayCount = serviceProvider.GetRequiredService<NavigationOptions>().MaxReplayItems;
        _current = this;
        
        foreach (var cache in _tempRegionCache)
        {
            if (cache.Value.Target.TryGetTarget(out var target) && target != null)
            {
                CreateRegion(cache.Key,
                    target,
                    cache.Value.ServiceProvider,
                    cache.Value.PreferCache);
            }
        }
        _tempRegionCache.Clear();
    }

    public IReadOnlyDictionary<string, IRegion> Regions
    {
        get
        {
            var dict = new Dictionary<string, IRegion>();
            foreach (var kv in _regions)
            {
                if (kv.Value.TryGetTarget(out var region))
                {
                    dict[kv.Key] = region;
                }
                else
                {
                    _regions.TryRemove(kv.Key, out _);
                }
            }
            return dict;
        }
    }

    public virtual async Task<NavigationResult> RequestNavigateAsync(
        string regionName,
        string viewName,
        INavigationParameters? navigationParameters = null,
        bool replay = false,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetRegion(regionName, out var region))
        {
            if (replay)
            {
                var queue = _pendingNavigations.GetOrAdd(regionName, _ => new ConcurrentQueue<(Func<Task<NavigationResult>>, TaskCompletionSource<NavigationResult>)>());
                var tcs = new TaskCompletionSource<NavigationResult>(TaskCreationOptions.RunContinuationsAsynchronously);

                queue.Enqueue((() => RequestNavigateAsync(regionName, viewName, navigationParameters, replay: false, cancellationToken), tcs));
                while (queue.Count > _maxReplayCount && queue.TryDequeue(out _)) { }

                Debug.WriteLine($"[Replay] Region '{regionName}' not found. Navigation request cached, waiting for region creation...");

                return await tcs.Task;
            }

            throw new InvalidOperationException($"Region '{regionName}' can not be found or has been collected.");
        }
        var context = new NavigationContext
        {
            RegionName = regionName,
            ViewName = viewName,
            Parameters = navigationParameters,
        };
        context.LinkCancellationToken(cancellationToken);

        if (context.CancellationToken.IsCancellationRequested)
            return NavigationResult.Cancelled(context);

        try
        {
            if (_currentRegion is not null && _currentRegion != region)
            {
                await _currentRegion.NavigateFromAsync(context);
            }
            await region.ActivateViewAsync(context);
            _currentRegion = region;
            return NavigationResult.Success(context);
        }
        catch (OperationCanceledException ex)
        {
            Debug.WriteLine($"{context} # Cancel:");
            Debug.WriteLine(ex);
            await region.RevertAsync(context);
            return NavigationResult.Cancelled(context);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{context} # Error:");
            Debug.WriteLine(ex);
            return NavigationResult.Failure(ex, context);
        }
    }

    public void AddRegion(string regionName, IRegion region)
    {
        if (!_regions.TryAdd(regionName, new WeakReference<IRegion>(region)))
            throw new InvalidOperationException($"Duplicated RegionName found: {regionName}");

        _ = TryReplayPendingNavigations(regionName);
    }

    public async Task<NavigationResult> GoForward(string regionName, CancellationToken cancellationToken = default)
    {
        var region = GetRegion(regionName);
        if (await region.CanGoForwardAsync())
        {
            try
            {
                await region.GoForwardAsync(cancellationToken);
                return NavigationResult.Success(TimeSpan.Zero);
            }
            catch (OperationCanceledException)
            {
                await region.RevertAsync(null);
                return NavigationResult.Cancelled();
            }
        }
        return NavigationResult.Failure(new NavigationException("Can not go forward!"));
    }

    public async Task<NavigationResult> GoBack(string regionName, CancellationToken cancellationToken = default)
    {
        var region = GetRegion(regionName);
        if (await region.CanGoBackAsync())
        {
            try
            {
                await region.GoBackAsync(cancellationToken);
                return NavigationResult.Success(TimeSpan.Zero);
            }
            catch (OperationCanceledException)
            {
                await region.RevertAsync(null);
                return NavigationResult.Cancelled();
            }
        }
        return NavigationResult.Failure(new NavigationException("Can not go back!"));
    }

    public Task<bool> CanGoForwardAsync(string regionName)
        => GetRegion(regionName).CanGoForwardAsync();

    public Task<bool> CanGoBackAsync(string regionName)
        => GetRegion(regionName).CanGoBackAsync();

    protected IRegion GetRegion(string regionName)
    {
        if (TryGetRegion(regionName, out var region))
        {
            return region!;
        }
        throw new InvalidOperationException($"Region '{regionName}' not found or has been collected.");
    }

    public bool TryGetRegion(string regionName, [MaybeNullWhen(false)] out IRegion region)
    {
        region = null;
        if (_regions.TryGetValue(regionName, out var weakRef))
        {
            if (weakRef.TryGetTarget(out var target))
            {
                region = target;
                return true;
            }
            else
            {
                _regions.TryRemove(regionName, out _);
            }
        }
        return false;
    }

    public bool TryRemoveRegion(string regionName, [MaybeNullWhen(false)] out IRegion region)
    {
        region = null;
        if (_regions.TryRemove(regionName, out var weakRef))
        {
            if (weakRef.TryGetTarget(out var target))
            {
                region = target;
                return true;
            }
        }
        return false;
    }

    protected void CreateRegion(
        string name,
        object target,
        IServiceProvider? serviceProvider,
        bool? preferCache)
    {
        if (TryGetRegion(name, out _))
            throw new InvalidOperationException($"Duplicated RegionName found: {name}");

        serviceProvider ??= _serviceProvider;
        var region = _regionFactory.CreateRegion(name, target, serviceProvider, preferCache);
        AddRegion(name, region);
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var kv in _regions)
        {
            if (kv.Value.TryGetTarget(out var region))
            {
                region.Dispose();
            }
        }
        _regions.Clear();
        _pendingNavigations.Clear();
        _tempRegionCache.Clear();
    }
    private async Task TryReplayPendingNavigations(string regionName)
    {
        if (_pendingNavigations.TryRemove(regionName, out var queue))
        {
            Debug.WriteLine($"[Replay] Found {queue.Count} cached navigations for '{regionName}', replaying...");
            while (queue.TryDequeue(out var item))
            {
                var (taskFactory, tcs) = item;
                var result = await taskFactory();
                tcs.TrySetResult(result);
            }
        }
    }


    protected static void OnAddRegionNameCore(string name, 
        object d,
        IServiceProvider? serviceProvider, 
        bool? preferCache)
    {
        if (_current == null)
        {
            if (!_tempRegionCache.TryAdd(
                name,
                (new WeakReference<object>(d), serviceProvider, preferCache)))
            {
                throw new InvalidOperationException($"Duplicated RegionName found: {name}");
            }
        }
        else
        {
            if (_current.TryGetRegion(name, out _))
                throw new InvalidOperationException($"Duplicated RegionName found:{name}");
            _current.CreateRegion(name, d, serviceProvider, preferCache);
        }
    }

    protected static void OnRemoveRegionNameCore(string name)
    {
        _current?.TryRemoveRegion(name, out _);
    }
}
