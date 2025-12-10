using AsyncNavigation.Abstractions;

namespace AsyncNavigation.Core;

public sealed class NavigationEventArgs : EventArgs
{
    public NavigationContext Context { get; }
    public IRegion Region { get; }

    public NavigationEventArgs(IRegion region, NavigationContext context)
    {
        Region = region;
        Context = context;
    }
}

