using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests.Mocks;

public class TestViewAwareNavigationAware : INavigationAware, IViewAware
{
    public IViewContext? LastViewContext { get; private set; }
    public int AttachedCount { get; private set; }
    public int DetachedCount { get; private set; }

    // INavigationAware
    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;
    public Task InitializeAsync(NavigationContext context) => Task.CompletedTask;
    public Task<bool> IsNavigationTargetAsync(NavigationContext context) => Task.FromResult(false);
    public Task OnNavigatedFromAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnNavigatedToAsync(NavigationContext context) => Task.CompletedTask;
    public Task OnUnloadAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // IViewAware
    public void OnViewAttached(IViewContext context)
    {
        LastViewContext = context;
        AttachedCount++;
    }

    public void OnViewDetached()
    {
        DetachedCount++;
    }
}
