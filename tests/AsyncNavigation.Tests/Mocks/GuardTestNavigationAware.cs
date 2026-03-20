using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests.Mocks;

public class GuardTestNavigationAware : INavigationAware, INavigationGuard
{
    public static GuardTestNavigationAware? LastCreated { get; private set; }

    public GuardTestNavigationAware()
    {
        LastCreated = this;
    }

    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;
    public bool AllowNavigation { get; set; } = true;
    public bool GuardWasCalled { get; private set; }
    public bool NavigatedFromWasCalled { get; private set; }

    public Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken cancellationToken)
    {
        GuardWasCalled = true;
        return Task.FromResult(AllowNavigation);
    }

    public Task InitializeAsync(NavigationContext context) => Task.CompletedTask;
    public Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(true);
    public Task OnNavigatedFromAsync(NavigationContext context) { NavigatedFromWasCalled = true; return Task.CompletedTask; }
    public Task OnNavigatedToAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnUnloadAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
