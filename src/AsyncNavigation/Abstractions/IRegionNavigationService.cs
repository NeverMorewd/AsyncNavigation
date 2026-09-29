namespace AsyncNavigation.Abstractions;

internal interface IRegionNavigationService<in T> : IDisposable where T : IRegionPresenter
{
    void SetCurrent(NavigationContext? context);
    void DetachCurrent(IView? view);
    void ForgetView(IView view);
    Task PreparePlacementAsync(NavigationContext context);
    Task CommitPlacementAsync(NavigationContext context, bool notify);
    Task RequestNavigateAsync(NavigationContext navigationContext, Action? onCompleted = null, bool coordinatePlacement = true);
    Task OnNavigateFromAsync(NavigationContext navigationContext);
    Task RevertAsync(NavigationContext? navigationContext);
}
