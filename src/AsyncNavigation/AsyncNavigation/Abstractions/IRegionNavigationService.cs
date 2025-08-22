using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

internal interface IRegionNavigationService<in T> where T : IRegionPresenter
{
    Task<NavigationResult> RequestNavigateAsync(NavigationContext navigationContext);
}
