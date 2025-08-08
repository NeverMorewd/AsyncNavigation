using AsyncNavigation.Core;

namespace AsyncNavigation.Abstractions;

public interface INavigationAware
{
    Task InitializeAsync(CancellationToken cancellationToken);
    Task OnNavigatedToAsync(NavigationContext context, CancellationToken cancellationToken);
    Task OnNavigatedFromAsync(NavigationContext context, CancellationToken cancellationToken);
    Task<bool> IsNavigationTargetAsync(NavigationContext context, CancellationToken cancellationToken);
}
