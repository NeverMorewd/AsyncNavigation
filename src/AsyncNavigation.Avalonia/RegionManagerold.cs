using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia;
using System.Collections.Concurrent;

namespace AsyncNavigation.Avalonia;

public sealed class RegionManagerold : 
    IRegionManager, 
    IObserver<AvaloniaPropertyChangedEventArgs<string>>,
    IDisposable
{
    #region RegionName
    public static readonly AttachedProperty<string> RegionNameProperty =
           AvaloniaProperty.RegisterAttached<RegionManagerold, AvaloniaObject, string>("RegionName");

    public static string GetRegionName(AvaloniaObject obj)
    {
        return obj.GetValue(RegionNameProperty);
    }

    public static void SetRegionName(AvaloniaObject obj, string value)
    {
        obj.SetValue(RegionNameProperty, value);
    }
    #endregion

    #region ServiceProvider
    public static readonly AttachedProperty<IServiceProvider> ServiceProviderProperty =
           AvaloniaProperty.RegisterAttached<RegionManagerold, AvaloniaObject, IServiceProvider>("ServiceProvider");

    public static IServiceProvider GetServiceProvider(AvaloniaObject obj)
    {
        return obj.GetValue(ServiceProviderProperty);
    }

    public static void SetServiceProvider(AvaloniaObject obj, IServiceProvider value)
    {
        obj.SetValue(ServiceProviderProperty, value);
    }
    #endregion

    #region PreferCache
    public static readonly AttachedProperty<bool?> PreferCacheProperty =
           AvaloniaProperty.RegisterAttached<RegionManagerold, AvaloniaObject, bool?>("PreferCache", null);

    public static bool? GetPreferCache(AvaloniaObject obj)
    {
        return obj.GetValue(PreferCacheProperty);
    }

    public static void SetPreferCache(AvaloniaObject obj, bool? value)
    {
        obj.SetValue(PreferCacheProperty, value);
    }
    #endregion

    private readonly IServiceProvider _serviceProvider;
    private readonly List<IDisposable> _subscriptions;
    private readonly ConcurrentDictionary<string, IRegion> _regions;
    private readonly IRegionFactory _regionFactory;
    private IRegion? _currentRegion;
    public RegionManagerold(IRegionFactory regionFactory, IServiceProvider serviceProvider)
    {
        _subscriptions = [];
        _regions = [];
        _serviceProvider = serviceProvider;
        _regionFactory = regionFactory;

        RegionNameProperty
            .Changed
            .Subscribe(this)
            .AddTo(_subscriptions);
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

    void IObserver<AvaloniaPropertyChangedEventArgs<string>>.OnCompleted()
    {

    }

    void IObserver<AvaloniaPropertyChangedEventArgs<string>>.OnError(Exception error)
    {
        throw error;
    }

    void IObserver<AvaloniaPropertyChangedEventArgs<string>>.OnNext(AvaloniaPropertyChangedEventArgs<string> value)
    {
        var name = value.NewValue.GetValueOrDefault();
        if (string.IsNullOrEmpty(name))
        {
            var old = value.OldValue.GetValueOrDefault();
            if (!string.IsNullOrEmpty(old))
            {
                _regions.TryRemove(old, out _);
            }
            return;
        }
        if (_regions.TryGetValue(name, out _))
            throw new InvalidOperationException($"Duplicated RegionName found:{name}");

        bool? useCache = null;
        IServiceProvider serviceProvider = _serviceProvider;
        
        if (value.Sender.IsSet(PreferCacheProperty))
        {
            useCache = value.Sender.GetValue(PreferCacheProperty);
        }
        if (value.Sender.IsSet(ServiceProviderProperty))
        {
            serviceProvider = value.Sender.GetValue(ServiceProviderProperty) ?? serviceProvider;
        }
        var region = _regionFactory.CreateRegion(name, value.Sender, serviceProvider, useCache);
        
        AddRegion(name, region);
    }

    public void Dispose()
    {
        _subscriptions?.DisposeAll();
        _regions.Values?.DisposeAll();
    }

    private bool TryRemoveRegion(object target)
    {
        if (target is AvaloniaObject aobj)
        {
            var regionName = GetRegionName(aobj);
            if (!string.IsNullOrEmpty(regionName) && _regions.TryRemove(regionName, out var region))
            {
                region.Dispose();
                return true;
            }
        }
        return false;
    }
}
