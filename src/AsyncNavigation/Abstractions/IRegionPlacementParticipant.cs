namespace AsyncNavigation.Abstractions;

/// <summary>
/// Provides the minimal operations required to temporarily move a rendered
/// navigation item out of a region and later put it back.
/// </summary>
/// <remarks>
/// These are low-level visual operations. Use IRegionPlacementNavigation to
/// coordinate a placement change with current-view state and navigation history.
/// </remarks>
public interface IRegionPlacementParticipant
{
    RegionPlacementItem Capture(Guid? navigationId = null);

    void Detach(RegionPlacementItem item);

    void Attach(RegionPlacementItem item, bool activate = true);
}
