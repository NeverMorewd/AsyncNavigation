using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public abstract partial class ViewModelBase : ReactiveObject, INavigationAware
{
    [Reactive]
    private string _name;
    [Reactive]
    private bool _isDialog = false;
    public ViewModelBase()
    {
        _name = GetType().Name;
    }

    public event AsyncEventHandler<AsyncEventArgs>? RequestUnloadAsync;

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public virtual async Task<bool> IsNavigationTargetAsync(NavigationContext context)
    {
        if (context.Parameters is not null)
        {
            if (context.Parameters.TryGetValue<bool>("requestNew", out var requestNew) && requestNew)
            {
                return false;
            }
        }
        if (TryGetDelay(context, out var delay))
        {
            await Task.Delay(delay!.Value, context.CancellationToken);
        }
        return true;
    }

    public virtual async Task OnNavigatedFromAsync(NavigationContext context)
    {
        if (TryGetDelay(context, out var delay))
        {
            await Task.Delay(delay!.Value, context.CancellationToken);
        }
    }

    public virtual async Task OnNavigatedToAsync(NavigationContext context)
    {
        if (TryGetDelay(context, out var delay))
        {
            await Task.Delay(delay!.Value, context.CancellationToken);
        }
    }

    public virtual Task OnUnloadAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected Task RequestUnload()
    {
        if (RequestUnloadAsync == null)
        {
            return Task.CompletedTask;
        }
        return RequestUnloadAsync!.Invoke(this, AsyncEventArgs.Empty);
    }

    private static bool TryGetDelay(NavigationContext navigationContext, out TimeSpan? delayTime)
    {
        if (navigationContext.Parameters is not null)
        {
            if (navigationContext.Parameters.TryGetValue<TimeSpan>("delay", out var delay))
            {
                delayTime = delay;
                return true;
            }
        }
        delayTime = null;
        return false;
    }
}
