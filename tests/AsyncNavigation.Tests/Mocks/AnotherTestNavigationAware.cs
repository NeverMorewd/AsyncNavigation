using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests.Mocks;

public class AnotherTestNavigationAware : INavigationAware
{
    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;

    public Task InitializeAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }

    public Task<bool> IsNavigationTargetAsync(NavigationContext context)
    {
        return Task.FromResult(true);
    }

    public Task OnNavigatedFromAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }

    public Task OnNavigatedToAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }

    public Task OnUnloadAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
