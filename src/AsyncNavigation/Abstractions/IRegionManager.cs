using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionManager
{
    IReadOnlyDictionary<string, IRegion> Regions { get; }
    void AddRegion(string regionName, IRegion region);

    Task<NavigationResult> RequestNavigateAsync(string regionName, 
        string viewName, 
        INavigationParameters? navigationParameters = null,
        CancellationToken cancellationToken = default);

    Task<bool> CanGoForwardAsync(string regionName);
    Task<bool> CanGoBackAsync(string regionName);
    Task<NavigationResult> GoForward(string regionName, CancellationToken cancellationToken = default);
    Task<NavigationResult> GoBack(string regionName, CancellationToken cancellationToken = default);
}
