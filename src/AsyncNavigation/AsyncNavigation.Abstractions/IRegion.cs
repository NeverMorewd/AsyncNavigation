using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation;

public interface IRegion : IAsyncDisposable
{
    IServiceProvider? ServiceProvider { get; set; }
    string Name { get; }
    INavigationAware? ActiveView { get; }
    IReadOnlyCollection<IView> Views { get; }
    INavigationHistory NavigationHistory { get; }
    bool IsInitialized { get; }
    Task<NavigationResult> ActivateViewAsync(NavigationContext navigationContext);
    Task<NavigationResult> DeactivateViewAsync(NavigationContext navigationContext);
    bool AddView(IView view);
    bool RemoveView(IView view);
    bool ContainsView(IView view);
    Task<bool> CanNavigateAsync(NavigationContext context);
    Task<bool> CanDeactivateAsync(CancellationToken cancellationToken = default);
    Task<bool> CanGoBackAsync();
    Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default);
    Task<bool> CanGoForwardAsync();
    Task<NavigationResult> GoForwardAsync(CancellationToken cancellationToken = default);
    Task InitializeAsync(CancellationToken cancellationToken = default);

    event AsyncEventHandler<ViewActivatedEventArgs<INavigationAware>>? ViewActivated;
    event AsyncEventHandler<ViewDeactivatedEventArgs<INavigationAware>>? ViewDeactivated;
    event AsyncEventHandler<ViewAddedEventArgs<INavigationAware>>? ViewAdded;
    event AsyncEventHandler<ViewRemovedEventArgs<INavigationAware>>? ViewRemoved;
    event AsyncEventHandler<NavigationFailedEventArgs>? NavigationFailed;
}
