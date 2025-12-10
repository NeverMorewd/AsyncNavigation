using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Diagnostics.CodeAnalysis;

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

    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;

    public virtual Task InitializeAsync(NavigationContext context)
    {
        return Task.CompletedTask;
    }

    public virtual Task<bool> IsNavigationTargetAsync(NavigationContext context)
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

    public virtual async Task OnNavigatedFromAsync(NavigationContext context)
    {
        if (TryGetDelay(context, out var delay))
        {
            await Task.Delay(delay!.Value, context.CancellationToken);
        }
    }

    public virtual async Task OnNavigatedToAsync(NavigationContext context)
    {
        if (GetRaiseError(context))
        {
            throw new Exception($"I am an Exception from {GetType()}");
        }
        if (TryGetDelay(context, out var delay))
        {
            await Task.Delay(delay!.Value, context.CancellationToken);
        }
    }

    public virtual Task OnUnloadAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task RequestUnloadAsync(CancellationToken cancellationToken)
    {
        if (AsyncRequestUnloadEvent == null)
        {
            return Task.CompletedTask;
        }
        return AsyncRequestUnloadEvent!.Invoke(this, AsyncEventArgs.Empty);
    }

    private static bool TryGetDelay(NavigationContext navigationContext, [MaybeNullWhen(false)] out TimeSpan? delayTime)
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

    private static bool GetRaiseError(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters is not null)
        {
            if (navigationContext.Parameters.TryGetValue<bool>("raiseError", out var raiseError))
            {
                return raiseError;
            }
        }
        return false;
    }
}
