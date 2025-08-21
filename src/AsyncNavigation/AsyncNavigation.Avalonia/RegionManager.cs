using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace AsyncNavigation.Avalonia;

public sealed class RegionManager : 
    IRegionManager, 
    IObserver<AvaloniaPropertyChangedEventArgs<string>>,
    IObserver<AvaloniaPropertyChangedEventArgs<IServiceProvider>>,
    IDisposable
{
    #region RegionName
    public static readonly AttachedProperty<string> RegionNameProperty =
           AvaloniaProperty.RegisterAttached<RegionManager, AvaloniaObject, string>("RegionName");

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
           AvaloniaProperty.RegisterAttached<RegionManager, AvaloniaObject, IServiceProvider>("ServiceProvider");

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
           AvaloniaProperty.RegisterAttached<RegionManager, AvaloniaObject, bool?>("PreferCache", null);

    public static bool? GetPreferCache(AvaloniaObject obj)
    {
        return obj.GetValue(PreferCacheProperty);
    }

    public static void SetPreferCache(AvaloniaObject obj, bool? value)
    {
        obj.SetValue(PreferCacheProperty, value);
    }
    #endregion

    static RegionManager()
    {

    }
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IDisposable> _subscriptions;
    private readonly ConcurrentDictionary<string, IRegion> _regions;
    private readonly IRegionFactory _regionFactory;
    public RegionManager(IRegionFactory regionFactory, IServiceProvider serviceProvider)
    {
        _subscriptions = [];
        _regions = [];
        _serviceProvider = serviceProvider;
        _regionFactory = regionFactory;

        RegionNameProperty
            .Changed
            .Subscribe(this)
            .AddTo(_subscriptions);
        ServiceProviderProperty
            .Changed
            .Subscribe(this)
            .AddTo(_subscriptions);
    }
    public IReadOnlyDictionary<string, IRegion> Regions => _regions;
    public IRegionManager AddToRegion(string regionName, string viewName)
    {
        if (_regions.TryGetValue(regionName, out IRegion? region))
        {
            var view = _serviceProvider.GetRequiredKeyedService<IView>(viewName);
            region.AddView(view);
        }
        return this;
    }
    public IRegionManager AddToRegion(string regionName, IView view)
    {
        if (_regions.TryGetValue(regionName, out IRegion? region))
        {
            region.AddView(view);
        }
        return this;
    }
    public async Task<NavigationResult> RequestNavigate(string regionName, 
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
            return await region.ActivateViewAsync(context);
        }
        throw new InvalidOperationException($"Region '{regionName}' not found.");
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(AvaloniaPropertyChangedEventArgs<string> value)
    {
        var name = value.NewValue.GetValueOrDefault();
        if (string.IsNullOrEmpty(name)) return;
        if (_regions.TryGetValue(name, out _))
            throw new InvalidOperationException($"Duplicated RegionName found:{name}");

        bool? useCache = null;
        if (value.Sender.IsSet(PreferCacheProperty))
        {
            useCache = value.Sender.GetValue(PreferCacheProperty);
        }
        var region = _regionFactory.CreateRegion(name, value.Sender, _serviceProvider, useCache);
        _regions.TryAdd(name, region);
    }

    public void OnNext(AvaloniaPropertyChangedEventArgs<IServiceProvider> value)
    {
        // todo support multiple serviceproviders
        throw new NotImplementedException("Todo: support multiple serviceproviders");
    }

    public void Dispose()
    {
        _subscriptions?.DisposeAll();
    }

    public void RemoveFromRegion(string regionName, string viewName)
    {
        if (_regions.TryGetValue(regionName, out IRegion? region))
        {
            //region.RemoveView(viewName);
        }
    }
}
