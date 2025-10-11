using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Threading;

namespace AsyncNavigation.Wpf;

public sealed class RegionManager : RegionManagerBase
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
            if (_current.TryGetRegion(name, out _))
                throw new InvalidOperationException($"Duplicated RegionName found:{name}");
            _current.CreateRegion(name, d, serviceProvider, preferCache);
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

    public RegionManager(IRegionFactory regionFactory, IServiceProvider serviceProvider):base(regionFactory, serviceProvider)
    {
        if (Volatile.Read(ref _current) != null)
            throw new InvalidOperationException("RegionManager is already created. Only one instance is allowed.");

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

    //public async override Task<NavigationResult> RequestNavigateAsync(string regionName, string viewName, INavigationParameters? navigationParameters = null, CancellationToken cancellationToken = default)
    //{
    //    var task = base.RequestNavigateAsync(regionName, viewName, navigationParameters, cancellationToken);
    //    task.ConfigureAwait(false).GetAwaiter().OnCompleted(() => { });
    //    return await task;
    //}
}
