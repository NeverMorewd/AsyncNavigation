using AsyncNavigation.Core;
using System.Diagnostics.CodeAnalysis;

namespace AsyncNavigation.Abstractions;

public interface IRegionManager
{
    IReadOnlyDictionary<string, IRegion> Regions { get; }
    void AddRegion(string regionName, IRegion region);
    bool TryGetRegion(string regionName, [MaybeNullWhen(false)] out IRegion region);
    bool TryRemoveRegion(string regionName, [MaybeNullWhen(false)] out IRegion region);
    event EventHandler<RegionChangeEventArgs> RegionChanged;

    /// <summary>
    /// RequestNavigateAsync
    /// </summary>
    /// <param name="regionName"></param>
    /// <param name="viewName"></param>
    /// <param name="navigationParameters"></param>
    /// <param name="replay"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<NavigationResult> RequestNavigateAsync(string regionName, 
        string viewName,
        INavigationParameters? navigationParameters = null,
        bool replay = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Navigates to the specified path asynchronously, with optional parameters and cancellation support.
    /// </summary>
    /// <remarks>This method is designed to support asynchronous navigation scenarios. The caller can use the
    /// <paramref name="navigationParameters"/>  to pass data to the target of the navigation. If <paramref
    /// name="replay"/> is set to <see langword="true"/>, the method will attempt  to replay the last navigation to the
    /// specified path, which can be useful in scenarios where the same navigation needs to be retried.</remarks>
    /// <param name="path">The navigation path to navigate to. This must be a valid path recognized by the navigation system.</param>
    /// <param name="navigationParameters">Optional parameters to pass to the target of the navigation. Can be <see langword="null"/> if no parameters are
    /// needed.</param>
    /// <param name="replay">Indicates whether the navigation should replay the last navigation to the specified path.  If <see
    /// langword="true"/>, the last navigation to the path will be replayed; otherwise, a new navigation will be
    /// initiated.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will be canceled if the token is triggered.</param>
    /// <returns>A task that represents the asynchronous navigation operation. The task result contains a <see
    /// cref="NavigationResult"/>  indicating the outcome of the navigation, including success or failure details.</returns>
    Task<NavigationResult> RequestPathNavigateAsync(string path,
        INavigationParameters? navigationParameters = null,
        bool replay = false,
        CancellationToken cancellationToken = default);

    Task<bool> CanGoForwardAsync(string regionName);
    Task<bool> CanGoBackAsync(string regionName);
    Task<NavigationResult> GoForward(string regionName, CancellationToken cancellationToken = default);
    Task<NavigationResult> GoBack(string regionName, CancellationToken cancellationToken = default);
}
