using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface IRegionNavigationService<in T> where T : IRegionPresenter
{
    Task<NavigationResult> RequestNavigateAsync(NavigationContext navigationContext);
}
