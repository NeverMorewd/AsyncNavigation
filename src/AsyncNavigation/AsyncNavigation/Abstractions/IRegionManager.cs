using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionManager
{
    IReadOnlyDictionary<string, IRegion> Regions { get; }
    void AddRegion(string regionName, IRegion region);

    Task<NavigationResult> RequestNavigate(string regionName, 
        string viewName, 
        INavigationParameters? navigationParameters = null,
        CancellationToken cancellationToken = default);

    Task<NavigationResult> GoForward(string regionName, CancellationToken cancellationToken = default);

    Task<NavigationResult> GoBack(string regionName, CancellationToken cancellationToken = default);
}
