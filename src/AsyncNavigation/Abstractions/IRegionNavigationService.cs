using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

internal interface IRegionNavigationService<in T> : IDisposable where T : IRegionPresenter
{
    Task<NavigationResult> RequestNavigateAsync(NavigationContext navigationContext);
    Task OnNavigateFromAsync(NavigationContext navigationContext);
}
