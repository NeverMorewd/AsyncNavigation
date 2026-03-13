using AsyncNavigation.Abstractions;
using Avalonia;
using Avalonia.Controls;

namespace AsyncNavigation.Avalonia;

public sealed class RegionManager : RegionManagerBase
{
    private static readonly IDisposable _subscription;

    // Tracks active IViewAware instances per region.
    // Single-page regions: at most one entry per key.
    // Multi-page regions (ItemsRegion): accumulates all active entries, cleared on Dispose.
    private readonly Dictionary<string, List<IViewAware>> _activeViewAwares = [];

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

        // NavigationPipelineMode.RenderFirst guarantees the view is in the visual tree
        // by the time OnNavigated fires. The fallback handles edge cases.
        if (navigationContext.Target.Value is not Visual visual) return;

        if (TopLevel.GetTopLevel(visual) is { } tl)
        {
            aware.OnViewAttached(new ViewContext(tl));
        }
        else
        {
            visual.AttachedToVisualTree += OnAttached;
            void OnAttached(object? s, VisualTreeAttachmentEventArgs e)
            {
                visual.AttachedToVisualTree -= OnAttached;
                if (TopLevel.GetTopLevel(visual) is { } topLevel)
                    aware.OnViewAttached(new ViewContext(topLevel));
            }
        }
    }

    public override void Dispose()
    {
        _subscription?.Dispose();
        foreach (var list in _activeViewAwares.Values)
            foreach (var aware in list)
                aware.OnViewDetached();
        _activeViewAwares.Clear();
        base.Dispose();
    }
}
