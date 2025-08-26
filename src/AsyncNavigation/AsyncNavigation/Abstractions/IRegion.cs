using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegion : IDisposable
{
    internal IRegionPresenter RegionPresenter { get; }
    INavigationHistory NavigationHistory { get; }
    Task<NavigationResult> ActivateViewAsync(NavigationContext navigationContext);
    Task<bool> CanGoBackAsync();
    Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default);
    Task<bool> CanGoForwardAsync();
    Task<NavigationResult> GoForwardAsync(CancellationToken cancellationToken = default);
}
