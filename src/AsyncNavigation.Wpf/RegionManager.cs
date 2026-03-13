using AsyncNavigation.Abstractions;
using System.Windows;

namespace AsyncNavigation.Wpf;

public sealed class RegionManager : RegionManagerBase
{
    // Tracks active IViewAware instances per region.
    // Single-page regions: at most one entry per key.
    // Multi-page regions (ItemsRegion): accumulates all active entries, cleared on Dispose.
    private readonly Dictionary<string, List<IViewAware>> _activeViewAwares = [];

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
        var oldName = e.OldValue as string;

        if (name == oldName)
            return;

        if (string.IsNullOrEmpty(name))
        {
            if (!string.IsNullOrEmpty(oldName))
                OnRemoveRegionNameCore(oldName);
            return;
        }

        var serviceProvider = GetServiceProvider(d);
        var preferCache = GetPreferCache(d);
        OnAddRegionNameCore(name, d, serviceProvider, preferCache);
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

    public RegionManager(IRegionFactory regionFactory,
        IServiceProvider serviceProvider) : base(regionFactory, serviceProvider)
    {
    }

    protected override void OnNavigated(string regionName, NavigationContext navigationContext)
    {
        base.OnNavigated(regionName, navigationContext);

        // For single-page regions (ContentRegion), detach the previously active ViewModel.
        if (TryGetRegion(regionName, out var region) && region.IsSinglePageRegion)
        {
            if (_activeViewAwares.TryGetValue(regionName, out var previous))
            {
                foreach (var old in previous)
                    old.OnViewDetached();
                previous.Clear();
            }
        }

        if (navigationContext.Target.Value?.DataContext is not IViewAware aware) return;

        if (!_activeViewAwares.TryGetValue(regionName, out var list))
            _activeViewAwares[regionName] = list = [];
        list.Add(aware);

        if (navigationContext.Target.Value is not FrameworkElement element) return;

        if (Window.GetWindow(element) is { } window)
        {
            aware.OnViewAttached(new ViewContext(window));
        }
        else
        {
            element.Loaded += OnLoaded;
            void OnLoaded(object s, RoutedEventArgs e)
            {
                element.Loaded -= OnLoaded;
                if (Window.GetWindow(element) is { } w)
                    aware.OnViewAttached(new ViewContext(w));
            }
        }
    }

    public override void Dispose()
    {
        foreach (var list in _activeViewAwares.Values)
            foreach (var aware in list)
                aware.OnViewDetached();
        _activeViewAwares.Clear();
        base.Dispose();
    }
}
