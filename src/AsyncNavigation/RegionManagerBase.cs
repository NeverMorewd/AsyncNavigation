using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;

namespace AsyncNavigation;

public abstract class RegionManagerBase : IRegionManager, IDisposable
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IRegionFactory _regionFactory;
    protected readonly ConcurrentDictionary<string, WeakReference<IRegion>> _regions;
    private IRegion? _currentRegion;

    protected RegionManagerBase(IRegionFactory regionFactory, IServiceProvider serviceProvider)
    {
        _regions = new ConcurrentDictionary<string, WeakReference<IRegion>>();
        _serviceProvider = serviceProvider;
        _regionFactory = regionFactory;
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

    public async Task<NavigationResult> RequestNavigateAsync(
        string regionName,
        string viewName,
        INavigationParameters? navigationParameters = null,
        CancellationToken cancellationToken = default)
    {
        var region = GetRegion(regionName);
        var context = new NavigationContext
        {
            RegionName = regionName,
            ViewName = viewName,
            Parameters = navigationParameters,
            CancellationToken = cancellationToken
        };

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
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            return NavigationResult.Cancelled(context);
        }
        catch (Exception ex)
        {
            return NavigationResult.Failure(ex, context);
        }
    }

    public void AddRegion(string regionName, IRegion region)
    {
        if (!_regions.TryAdd(regionName, new WeakReference<IRegion>(region)))
            throw new InvalidOperationException($"Duplicated RegionName found: {regionName}");
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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
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

    public bool TryGetRegion(string regionName, out IRegion? region)
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

    public bool TryRemoveRegion(string regionName, out IRegion? region)
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
    }
}
