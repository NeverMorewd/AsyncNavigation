using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests.Mocks;

public class TestNavigationAware : INavigationAware
{
    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;
    public Task InitializeAsync(NavigationContext context) => Task.CompletedTask;
    public Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(true);
    public Task OnNavigatedFromAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnNavigatedToAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnUnloadAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
