namespace AsyncNavigation.Abstractions;

/// <summary>Synchronizes placement changes with a region's current view and history.</summary>
public interface IRegionPlacementNavigation
{
    void OnViewDetached(RegionPlacementItem item);
    void OnViewClosed(RegionPlacementItem item);
    /// <summary>Transfers an existing view transactionally. The caller must hold the region placement lease.</summary>
    Task RestorePlacementAsync(RegionPlacementItem item, Func<Task> transferContent,
        Func<Task> rollbackContent, CancellationToken cancellationToken = default);
}
