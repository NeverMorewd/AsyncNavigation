using AsyncNavigation.Abstractions;

namespace AsyncNavigation;

public class RegionManagerAccessor : IRegionManagerAccessor
{
    private WeakReference<IRegionManager>? _current;

    public IRegionManager? Current
    {
        get
        {
            if (_current is { } wr && wr.TryGetTarget(out var manager))
                return manager;
            return null;
        }
    }

    public void SetCurrent(IRegionManager manager)
    {
        _current = new WeakReference<IRegionManager>(manager);
    }
}
