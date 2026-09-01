namespace AsyncNavigation.Abstractions;

/// <summary>
/// Allows a region indicator host to temporarily lend its rendered view to
/// another visual host without resolving or creating the view again.
/// </summary>
public interface IRegionPlacementContentHost
{
    object DetachContent();

    void AttachContent(object content);
}
