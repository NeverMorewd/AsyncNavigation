namespace AsyncNavigation.Abstractions;

internal interface IRegionNavigationService<in T> : IDisposable where T : IRegionPresenter
{
    Task RequestNavigateAsync(NavigationContext navigationContext);
    Task OnNavigateFromAsync(NavigationContext navigationContext);
    Task RevertAsync(NavigationContext? navigationContext);
}
