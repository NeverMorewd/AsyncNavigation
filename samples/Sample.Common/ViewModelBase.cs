using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public abstract partial class ViewModelBase : ReactiveObject, INavigationAware
{
    [Reactive]
    private string _name;
    public ViewModelBase()
    {
        _name = GetType().Name;
    }

    public event AsyncEventHandler<EventArgs>? RequestUnloadAsync;

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task<bool> IsNavigationTargetAsync(NavigationContext context, CancellationToken cancellationToken)
    {
        if (context.Parameters is not null)
        {
            if (context.Parameters.TryGetValue<bool>("requestNew", out var requestNew) && requestNew)
            {
                return Task.FromResult(false);
            }
        }
        return Task.FromResult(true);
    }

    public virtual Task OnNavigatedFromAsync(NavigationContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnNavigatedToAsync(NavigationContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected Task RequestUnload()
    {
        if(RequestUnloadAsync == null)
        {
            return Task.CompletedTask;
        }
        return RequestUnloadAsync!.Invoke(this, EventArgs.Empty);
    }
}
