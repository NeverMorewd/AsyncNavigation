using AsyncNavigation.Abstractions;
using Avalonia;

namespace AsyncNavigation.Avalonia;

public sealed class RegionManager : RegionManagerBase,
    IObserver<AvaloniaPropertyChangedEventArgs<string>>
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

    private readonly List<IDisposable> _subscriptions = [];

    public RegionManager(IRegionFactory regionFactory, IServiceProvider serviceProvider)
        : base(regionFactory, serviceProvider)
    {
        RegionNameProperty.Changed
            .Subscribe(this)
            .AddTo(_subscriptions);
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
                TryRemoveRegion(old, out _);
            }
            return;
        }
        if (TryGetRegion(name, out _))
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

        CreateRegion(name, value.Sender, serviceProvider, useCache);
    }

    public override void Dispose()
    {
        base.Dispose();
        _subscriptions.DisposeAll();
    }
}
