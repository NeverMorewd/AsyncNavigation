using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionManager
{
    IReadOnlyDictionary<string, IRegion> Regions { get; }
    IRegionManager AddToRegion(string regionName, string viewName);
    void RemoveFromRegion(string regionName, string viewName);
    Task<NavigationResult> RequestNavigate(string regionName, 
        string viewName, 
        INavigationParameters? navigationParameters = null,
        CancellationToken cancellationToken = default);

    
}
