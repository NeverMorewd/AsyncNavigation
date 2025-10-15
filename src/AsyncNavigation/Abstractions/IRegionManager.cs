using AsyncNavigation.Core;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation.Abstractions;

public interface IRegionManager
{
    IReadOnlyDictionary<string, IRegion> Regions { get; }
    void AddRegion(string regionName, IRegion region);
    bool TryGetRegion(string regionName, [MaybeNullWhen(false)] out IRegion region);
    bool TryRemoveRegion(string regionName, [MaybeNullWhen(false)] out IRegion region);


    Task<NavigationResult> RequestNavigateAsync(string regionName, 
        string viewName,
        INavigationParameters? navigationParameters = null,
        bool replay = false,
        CancellationToken cancellationToken = default);

    Task<bool> CanGoForwardAsync(string regionName);
    Task<bool> CanGoBackAsync(string regionName);
    Task<NavigationResult> GoForward(string regionName, CancellationToken cancellationToken = default);
    Task<NavigationResult> GoBack(string regionName, CancellationToken cancellationToken = default);
}
