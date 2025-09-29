using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;
using System.Windows;

namespace AsyncNavigation.Wpf;

public sealed class RegionManager : IRegionManager, IDisposable
{
    private static readonly ConcurrentDictionary<string, (WeakReference<DependencyObject> Target, IServiceProvider ServiceProvider, bool? PreferCache)> _tempRegionCache = [];
    private static RegionManager? _current;
    #region RegionName
    public static readonly DependencyProperty RegionNameProperty =
         DependencyProperty.RegisterAttached(
             "RegionName",
             typeof(string),
             typeof(RegionManager),
             new PropertyMetadata(null, OnRegionNameChanged));

    private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var name = e.NewValue as string;

        if (string.IsNullOrEmpty(name))
        {
            var oldName = e.OldValue as string;
            if (!string.IsNullOrEmpty(oldName))
                _tempRegionCache.TryRemove(oldName, out _);
            return;
        }
        var serviceProvider = GetServiceProvider(d);
        var preferCache = GetPreferCache(d);
        if (_current == null)
        {
            if (!_tempRegionCache.TryAdd(
                name,
                (new WeakReference<DependencyObject>(d), serviceProvider, preferCache)))
            {
                throw new InvalidOperationException($"Duplicated RegionName found: {name}");
            }
        }
        else
        {
            _current.CreateRegionCore(name, d, serviceProvider, preferCache);
        }
    }

    public static string GetRegionName(DependencyObject obj)
    {
        return (string)obj.GetValue(RegionNameProperty);
    }

    public static void SetRegionName(DependencyObject obj, string value)
    {
        obj.SetValue(RegionNameProperty, value);
    }
    #endregion

    #region ServiceProvider
    public static readonly DependencyProperty ServiceProviderProperty =
           DependencyProperty.RegisterAttached("ServiceProvider",
             typeof(IServiceProvider),
             typeof(RegionManager),
             new PropertyMetadata(null));

    public static IServiceProvider GetServiceProvider(DependencyObject obj)
    {
        return (IServiceProvider)obj.GetValue(ServiceProviderProperty);
    }

    public static void SetServiceProvider(DependencyObject obj, IServiceProvider value)
    {
        obj.SetValue(ServiceProviderProperty, value);
    }
    #endregion

    #region PreferCache
    public static readonly DependencyProperty PreferCacheProperty =
           DependencyProperty.RegisterAttached("PreferCache",
             typeof(bool?),
             typeof(RegionManager),
             new PropertyMetadata(null));

    public static bool? GetPreferCache(DependencyObject obj)
    {
        return (bool?)obj.GetValue(PreferCacheProperty);
    }

    public static void SetPreferCache(DependencyObject obj, bool? value)
    {
        obj.SetValue(PreferCacheProperty, value);
    }
    #endregion

    private readonly IServiceProvider _serviceProvider;
    private readonly List<IDisposable> _subscriptions;
    private readonly ConcurrentDictionary<string, IRegion> _regions;
    private readonly IRegionFactory _regionFactory;
    private IRegion? _currentRegion;
    public RegionManager(IRegionFactory regionFactory, IServiceProvider serviceProvider)
    {
        if (Volatile.Read(ref _current) != null)
            throw new InvalidOperationException("RegionManager is already created. Only one instance is allowed.");

        _subscriptions = [];
        _regions = [];
        _serviceProvider = serviceProvider;
        _regionFactory = regionFactory;
        _current = this;

        foreach (var cache in _tempRegionCache)
        {
            if (cache.Value.Target.TryGetTarget(out var target) && target != null)
            {
                CreateRegionCore(cache.Key,
                    target,
                    cache.Value.ServiceProvider,
                    cache.Value.PreferCache);
            }
        }
        _tempRegionCache.Clear();
    }
    public IReadOnlyDictionary<string, IRegion> Regions => _regions;
    public async Task<NavigationResult> RequestNavigateAsync(string regionName,
        string viewName,
        INavigationParameters? navigationParameters = null,
        CancellationToken cancellationToken = default)
    {
        if (_regions.TryGetValue(regionName, out IRegion? region))
        {
            var context = new NavigationContext()
            {
                RegionName = regionName,
                ViewName = viewName,
                Parameters = navigationParameters,
                CancellationToken = cancellationToken
            };
            if (_currentRegion is not null && _currentRegion != region)
            {
                await _currentRegion.NavigateFromAsync(context);
            }
            var result = await region.ActivateViewAsync(context);
            if (result.IsSuccessful)
            {
                _currentRegion = region;
            }
            return result;
        }
        throw new InvalidOperationException($"Region '{regionName}' not found.");
    }
    public void AddRegion(string regionName, IRegion region)
    {
        if (_regions.TryGetValue(regionName, out _))
            throw new InvalidOperationException($"Duplicated RegionName found:{regionName}");
        if (!_regions.TryAdd(regionName, region))
            throw new InvalidOperationException($"Duplicated RegionName found:{regionName}");

    }
    public Task<NavigationResult> GoForward(string regionName, CancellationToken cancellationToken = default)
    {
        return GetRegion(regionName).GoForwardAsync(cancellationToken);
    }

    public Task<NavigationResult> GoBack(string regionName, CancellationToken cancellationToken = default)
    {
        return GetRegion(regionName).GoBackAsync(cancellationToken);
    }
    public Task<bool> CanGoForwardAsync(string regionName)
    {
        return GetRegion(regionName).CanGoForwardAsync();
    }

    public Task<bool> CanGoBackAsync(string regionName)
    {
        return GetRegion(regionName).CanGoBackAsync();
    }

    private IRegion GetRegion(string regionName)
    {
        if (_regions.TryGetValue(regionName, out IRegion? region))
        {
            return region;
        }
        throw new InvalidOperationException($"Region '{regionName}' not found.");
    }

    private void CreateRegionCore(string name, DependencyObject target, IServiceProvider serviceProvider, bool? preferCache)
    {
        if (_regions.TryGetValue(name, out _))
            throw new InvalidOperationException($"Duplicated RegionName found:{name}");

        serviceProvider ??= _serviceProvider;

        var region = _regionFactory.CreateRegion(name, target, serviceProvider, preferCache);
        AddRegion(name, region);
    }
    public void Dispose()
    {
        _subscriptions?.DisposeAll();
        _regions.Values?.DisposeAll();
    }

    private bool TryRemoveRegion(object target)
    {
        if (target is DependencyObject dobj)
        {
            var regionName = GetRegionName(dobj);
            if (!string.IsNullOrEmpty(regionName) && _regions.TryRemove(regionName, out var region))
            {
                region.Dispose();
                return true;
            }
        }
        return false;
    }
}
