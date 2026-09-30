using AsyncNavigation;
using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using AsyncNavigation.Floating;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Sample.Common;

public abstract partial class ViewModelBase : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private bool _isDialog = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FloatButtonText))]
    private bool _isFloating;

    public string FloatButtonText => IsFloating ? "Dock to region" : "Float";

    public ViewModelBase()
    {
        _name = GetType().Name;
    }

    public event AsyncEventHandler<AsyncEventArgs>? AsyncRequestUnloadEvent;

    public string? RegionName { get; private set; }
    public Guid NavigationId { get; private set; }

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
        RegionName = context.RegionName;
        NavigationId = context.NavigationId;
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

    protected Task UnloadOrCloseFloatingAsync(IViewPlacementService? viewPlacementService, CancellationToken cancellationToken = default)
    {
        var session = FindFloatingSession(viewPlacementService);
        return session is not null ? session.CloseAsync(cancellationToken) : RequestUnloadAsync(cancellationToken);
    }

    /// <summary>
    /// Floats this view if it isn't already floating, or docks it back into its region if it is.
    /// Drives the floating window's own dock affordance (<see cref="FloatingWindowOptions.ShowDockButton"/>
    /// is set to <see langword="false"/>), so the view controls both directions through one button.
    /// </summary>
    protected async Task FloatOrDockAsync(IViewPlacementService? viewPlacementService, CancellationToken cancellationToken = default)
    {
        var session = FindFloatingSession(viewPlacementService);
        if (session is not null)
        {
            await session.RestoreAsync(cancellationToken);
            return;
        }
        if (viewPlacementService is null || RegionName is null)
            return;

        var newSession = await viewPlacementService.FloatAsync(
            RegionName, NavigationId, new FloatingWindowOptions { ShowDockButton = false }, cancellationToken);
        IsFloating = true;
        newSession.StateChanged += OnFloatingSessionStateChanged;
    }

    private void OnFloatingSessionStateChanged(object? sender, EventArgs e)
    {
        if (sender is not IFloatingViewSession { State: ViewPlacementState.Restored or ViewPlacementState.Closed } session)
            return;
        session.StateChanged -= OnFloatingSessionStateChanged;
        IsFloating = false;
    }

    private IFloatingViewSession? FindFloatingSession(IViewPlacementService? viewPlacementService) =>
        viewPlacementService?.FloatingViews.FirstOrDefault(s => s.NavigationId == NavigationId);

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
