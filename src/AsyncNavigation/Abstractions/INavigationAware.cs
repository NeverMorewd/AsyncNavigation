using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface INavigationAware
{
    Task InitializeAsync(NavigationContext context);
    Task OnNavigatedToAsync(NavigationContext context);
    Task OnNavigatedFromAsync(NavigationContext context);   
    Task<bool> IsNavigationTargetAsync(NavigationContext context);
    Task OnUnloadAsync(CancellationToken cancellationToken);
    event AsyncEventHandler<AsyncEventArgs> AsyncRequestUnloadEvent;
}
