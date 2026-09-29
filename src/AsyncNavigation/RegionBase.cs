using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace AsyncNavigation;

public abstract class RegionBase<TRegion, TControl> : IRegion, IRegionPresenter, IRegionPlacementNavigation
    where TRegion : class, IRegionPresenter
    where TControl : class
{
    private readonly ViewPlacementCoordinator? _placement;
    private readonly IRegionNavigationService<TRegion> _regionNavigationService;
    private readonly IRegionNavigationHistory _navigationHistory;
    private readonly IRegionControlAccessor<TControl> _controlAccessor;
    protected readonly RegionContext _context = new();
    public RegionBase(string name, TControl control, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _placement = serviceProvider.GetService<ViewPlacementCoordinator>();
        Name = name;
        _controlAccessor = new WeakRegionControlAccessor<TControl>(control);
        _regionNavigationService = serviceProvider.GetRequiredService<IRegionNavigationServiceFactory>().Create((this as TRegion)!);
        _navigationHistory = serviceProvider.GetRequiredService<IRegionNavigationHistory>();

        RegionControlAccessor.ExecuteOn(InitializeOnRegionCreated);

    }

    IRegionPresenter IRegion.RegionPresenter => this;
    public bool EnableViewCache { get; protected set; }
    public bool IsSinglePageRegion { get; protected set; }
    public abstract NavigationPipelineMode NavigationPipelineMode { get; }
    public IRegionControlAccessor<TControl> RegionControlAccessor => _controlAccessor;

    public string Name
    {
        get;
        protected set;
    }
    public event EventHandler<NavigationEventArgs>? Navigated;
    #region IRegion Methods
    /// <summary>
    /// Performs initialization logic when a region is created and associated with the specified control.
    /// Binding logic or setup tasks related to the control can be implemented in this method.
    /// </summary>
    /// <remarks>This method is intended to be overridden in a derived class to provide custom initialization
    /// logic. The base implementation does not perform any actions.</remarks>
    /// <param name="control">The control associated with the newly created region. This parameter cannot be null.</param>
    protected virtual void InitializeOnRegionCreated(TControl control)
    {
        
    }
    async Task<NavigationResult> IRegion.ActivateViewAsync(NavigationContext navigationContext)
    {
        await _regionNavigationService.RequestNavigateAsync(navigationContext, () =>
        {
            if (!navigationContext.ActivatedExternally)
                _navigationHistory.Add(navigationContext);
            navigationContext.UpdateStatus(NavigationStatus.Succeeded);
        });
        var result = NavigationResult.Success(navigationContext);
        if (!navigationContext.ActivatedExternally)
            RaiseNavigated(navigationContext);
        return result;
    }
    Task<bool> IRegion.CanGoBackAsync()
    {
        return Task.FromResult(_navigationHistory.CanGoBack);
    }

    public async Task<NavigationResult> GoBackAsync(CancellationToken cancellationToken = default)
    {
        using var lease = _placement is null ? null : await _placement.EnterAsync(Name, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var navigationContext = _navigationHistory.GoBack() ?? throw new NavigationException("Cannot go back!");
        navigationContext.IsBackNavigation = true;
        navigationContext.LinkCancellationToken(cancellationToken);
        try
        {
            await _regionNavigationService.RequestNavigateAsync(navigationContext, coordinatePlacement: false);
        }
        catch
        {
            _navigationHistory.GoForward();
            throw;
        }
        if (navigationContext.ActivatedExternally)
            _navigationHistory.GoForward();
        navigationContext.UpdateStatus(NavigationStatus.Succeeded);
        var result = NavigationResult.Success(navigationContext);
        if (!navigationContext.ActivatedExternally)
            RaiseNavigated(navigationContext);
        return result;
    }

    Task<bool> IRegion.CanGoForwardAsync()
    {
        return Task.FromResult(_navigationHistory.CanGoForward);
    }

    public async Task<NavigationResult> GoForwardAsync(CancellationToken cancellationToken = default)
    {
        using var lease = _placement is null ? null : await _placement.EnterAsync(Name, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var navigationContext = _navigationHistory.GoForward() ?? throw new NavigationException("Cannot go forward!");
        navigationContext.IsForwardNavigation = true;
        navigationContext.LinkCancellationToken(cancellationToken);
        try
        {
            await _regionNavigationService.RequestNavigateAsync(navigationContext, coordinatePlacement: false);
        }
        catch
        {
            _navigationHistory.GoBack();
            throw;
        }
        if (navigationContext.ActivatedExternally)
            _navigationHistory.GoBack();
        navigationContext.UpdateStatus(NavigationStatus.Succeeded);
        var result = NavigationResult.Success(navigationContext);
        if (!navigationContext.ActivatedExternally)
            RaiseNavigated(navigationContext);
        return result;
    }
    Task IRegion.NavigateFromAsync(NavigationContext navigationContext)
    {
        return _regionNavigationService.OnNavigateFromAsync(navigationContext);
    }

    /// <summary>
    /// RevertAsync
    /// Should not raise Navigated event!
    /// </summary>
    /// <param name="navigationContext"></param>
    /// <returns></returns>
    Task IRegion.RevertAsync(NavigationContext? navigationContext)
    {
        return _regionNavigationService.RevertAsync(navigationContext);
    }
    public void OnViewDetached(RegionPlacementItem item)
    {
        _regionNavigationService.DetachCurrent(item.Context.Target.Value);
        // Multi-item regions can select a different view when an item is detached.
        if (!IsSinglePageRegion && this is IRegionPlacementParticipant participant)
        {
            try { _regionNavigationService.SetCurrent(participant.Capture().Context); }
            catch (InvalidOperationException) { }
        }
    }

    public void OnViewClosed(RegionPlacementItem item)
    {
        if (item.Context.Target.Value is not { } view) return;
        _regionNavigationService.ForgetView(view);
        _navigationHistory.RemoveView(view);
    }

    public async Task RestorePlacementAsync(RegionPlacementItem item, Func<Task> transferContent,
        Func<Task> rollbackContent, CancellationToken cancellationToken = default)
    {
        if (this is not IRegionPlacementParticipant participant)
            throw new NotSupportedException("The region does not support placement.");
        RegionPlacementItem? previous = null;
        try { previous = participant.Capture(); }
        catch (InvalidOperationException) { }

        var context = new NavigationContext
        {
            RegionName = Name,
            ViewName = item.Context.ViewName,
            Parameters = item.Context.Parameters
        };
        context.Target.Value = item.Context.Target.Value!;
        context.IndicatorHost.Value = item.Context.IndicatorHost.Value!;
        context.LinkCancellationToken(cancellationToken);
        var restored = new RegionPlacementItem(context, item.Index, item.WasSelected);
        var attached = false;
        var detached = false;
        var transferred = false;
        object? displacedContent = null;
        var sharedHost = previous is not null &&
            ReferenceEquals(previous.Context.IndicatorHost.Value, item.Context.IndicatorHost.Value)
            ? previous.Context.IndicatorHost.Value as IRegionPlacementContentHost : null;
        try
        {
            if (previous is not null)
                await _regionNavigationService.PreparePlacementAsync(context);
            cancellationToken.ThrowIfCancellationRequested();
            if (IsSinglePageRegion && previous is not null)
            {
                participant.Detach(previous);
                detached = true;
                displacedContent = sharedHost?.DetachContent();
            }
            transferred = true;
            await transferContent();
            participant.Attach(restored);
            attached = true;
            await _regionNavigationService.CommitPlacementAsync(context, notify: previous is not null);
            cancellationToken.ThrowIfCancellationRequested();
            _navigationHistory.Add(context);
            context.UpdateStatus(NavigationStatus.Succeeded);
        }
        catch (Exception ex)
        {
            if (attached) participant.Detach(restored);
            if (transferred) await rollbackContent();
            if (displacedContent is not null) sharedHost!.AttachContent(displacedContent);
            if (detached) participant.Attach(previous!);
            else if (previous is not null) await ProcessActivateAsync(previous.Context);
            _regionNavigationService.SetCurrent(previous?.Context);
            context.UpdateStatus(ex is OperationCanceledException ? NavigationStatus.Cancelled : NavigationStatus.Failed, ex);
            throw;
        }
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        _regionNavigationService.Dispose();
        _navigationHistory.Clear();
    }
    #endregion
    public abstract Task ProcessActivateAsync(NavigationContext navigationContext);
    public abstract Task ProcessDeactivateAsync(NavigationContext? navigationContext);

    private void RaiseNavigated(NavigationContext context)
    {
        Navigated?.Invoke(this, new NavigationEventArgs(this, context));
    }


#if DEBUG
    ~RegionBase()
    {
        Debug.WriteLine($"{Name} was collected!");
    }
#endif
}
