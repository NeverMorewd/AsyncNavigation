using AsyncNavigation.Abstractions;
using Avalonia;

namespace AsyncNavigation.Avalonia;

public sealed class RegionManager : RegionManagerBase
{
    private static readonly IDisposable _subscription;

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
    public static readonly AttachedProperty<IServiceProvider?> ServiceProviderProperty =
           AvaloniaProperty.RegisterAttached<RegionManager, AvaloniaObject, IServiceProvider?>("ServiceProvider", defaultValue: null);

    public static IServiceProvider? GetServiceProvider(AvaloniaObject obj)
    {
        return obj.GetValue(ServiceProviderProperty);
    }

    public static void SetServiceProvider(AvaloniaObject obj, IServiceProvider? value)
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
        _subscription = RegionNameProperty
            .Changed
            .AddClassHandler<AvaloniaObject, string>((target, args) => 
            {
                var name = args.NewValue.GetValueOrDefault();
                var old = args.OldValue.GetValueOrDefault();

                if (name == old)
                    return;

                if (string.IsNullOrEmpty(name))
                {
                    if (!string.IsNullOrEmpty(old))
                    {
                        OnRemoveRegionNameCore(old);
                    }
                    return;
                }
                bool? useCache = null;
                if (args.Sender.IsSet(PreferCacheProperty))
                {
                    useCache = args.Sender.GetValue(PreferCacheProperty);
                }
                var serviceProvider = args.Sender.GetValue(ServiceProviderProperty);

                OnAddRegionNameCore(name, target, serviceProvider, useCache);
            });
    }

    public RegionManager(IRegionFactory regionFactory, 
        IServiceProvider serviceProvider) : base(regionFactory, serviceProvider)
    {

    }

    public override void Dispose()
    {
        base.Dispose();
        _subscription?.Dispose();
    }
}
