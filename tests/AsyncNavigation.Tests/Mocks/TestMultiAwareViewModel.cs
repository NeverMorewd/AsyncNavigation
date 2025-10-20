using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests.Mocks;

public class TestMultiAwareViewModel : INavigationAware, IDialogAware
{
    public string Title => nameof(TestMultiAwareViewModel);

    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;
    public event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;

    public Task InitializeAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }

    public Task<bool> IsNavigationTargetAsync(NavigationContext context)
    {
        return Task.FromResult(true);
    }

    public Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnDialogOpenedAsync(IDialogParameters? parameters, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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
