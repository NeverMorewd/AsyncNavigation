using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;

namespace AsyncNavigation.Tests.Mocks;

public class DummyNavigationView : IView
{
    public object? DataContext { get; set; }
}
public class DummyNavigationViewModel : INavigationAware
{
    public event AsyncEventHandler<AsyncEventArgs> AsyncRequestUnloadEvent;

    public Task InitializeAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsNavigationTargetAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task OnNavigatedFromAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task OnNavigatedToAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task OnUnloadAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
public class DummyDialogView : IView
{
    public object? DataContext { get; set; }
}
public class DummyDialogViewModel : IDialogAware
{
    public string Title => throw new NotImplementedException();

    public event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;

    public Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task OnDialogOpenedAsync(IDialogParameters? parameters, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
public class DummyComboView : IView
{
    public object? DataContext { get; set; }
}
public class DummyComboViewModel : INavigationAware, IDialogAware
{
    public string Title => throw new NotImplementedException();

    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;
    public event AsyncEventHandler<DialogCloseEventArgs>? RequestCloseAsync;

    public Task InitializeAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsNavigationTargetAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task OnDialogClosedAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task OnDialogClosingAsync(IDialogResult? dialogResult, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task OnDialogOpenedAsync(IDialogParameters? parameters, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task OnNavigatedFromAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task OnNavigatedToAsync(NavigationContext context)
    {
        throw new NotImplementedException();
    }

    public Task OnUnloadAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}