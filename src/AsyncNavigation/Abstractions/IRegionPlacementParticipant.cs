namespace AsyncNavigation.Abstractions;

/// <summary>
/// Provides the minimal operations required to temporarily move a rendered
/// navigation item out of a region and later put it back.
/// </summary>
/// <remarks>
/// Placement changes are not navigation operations and therefore must not
/// invoke navigation lifecycle callbacks or modify navigation history.
/// </remarks>
public interface IRegionPlacementParticipant
{
    RegionPlacementItem Capture(Guid? navigationId = null);

    void Detach(RegionPlacementItem item);

    void Attach(RegionPlacementItem item, bool activate = true);
}
