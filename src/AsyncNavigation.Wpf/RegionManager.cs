using AsyncNavigation.Abstractions;
using System.Windows;

namespace AsyncNavigation.Wpf;

public sealed class RegionManager : RegionManagerBase
{
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
}
