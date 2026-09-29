namespace AsyncNavigation;

/// <summary>
/// Describes a rendered navigation item and its original position in a region.
/// </summary>
public sealed class RegionPlacementItem
{
    public RegionPlacementItem(NavigationContext context, int index, bool wasSelected)
    {
        ArgumentNullException.ThrowIfNull(context);
        Context = context;
        Index = index;
        WasSelected = wasSelected;
    }

    public NavigationContext Context { get; }

    public int Index { get; }

    public bool WasSelected { get; }
}
