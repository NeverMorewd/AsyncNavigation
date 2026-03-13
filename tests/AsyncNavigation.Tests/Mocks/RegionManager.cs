using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Tests.Mocks;

internal class RegionManager : RegionManagerBase
{
    private readonly Dictionary<string, List<IViewAware>> _activeViewAwares = [];

    public RegionManager(IRegionFactory regionFactory, IServiceProvider serviceProvider)
        : base(regionFactory, serviceProvider)
    {
    }

    protected override void OnNavigated(string regionName, NavigationContext navigationContext)
    {
        base.OnNavigated(regionName, navigationContext);

        // Single-page region: detach the previous ViewModel before attaching the new one.
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

        // Use a test-only ViewContext (no platform dependency).
        aware.OnViewAttached(new TestViewContext());
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
