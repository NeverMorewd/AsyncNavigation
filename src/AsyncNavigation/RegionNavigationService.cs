using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace AsyncNavigation;

internal sealed class RegionNavigationService<T> : IRegionNavigationService<T> where T : IRegionPresenter
{
    private readonly ViewPlacementCoordinator? _placement;
    private readonly IViewManager _viewCacheManager;
    private readonly IRegionIndicatorManager _regionIndicatorManager;
    private readonly IAsyncJobProcessor _navigationJobScheduler;
    private readonly IRegionPresenter _regionPresenter;
    private (IView View, NavigationContext NavigationContext)? _current;
    private readonly NavigationJobStrategy _navigationJobStrategy;
    public RegionNavigationService(T regionPresenter, IServiceProvider serviceProvider)
    {
        _placement = serviceProvider.GetService<ViewPlacementCoordinator>();
        _regionPresenter = regionPresenter;
        _navigationJobScheduler = serviceProvider.GetRequiredService<IAsyncJobProcessor>();
        _viewCacheManager = serviceProvider.GetRequiredService<IViewManager>();
        _regionIndicatorManager = serviceProvider.GetRequiredService<IRegionIndicatorManager>();
        _navigationJobStrategy = serviceProvider.GetRequiredService<NavigationOptions>()!.NavigationJobStrategy;
    }
    internal (IView View, NavigationContext NavigationContext)? Current
    {
        get => _current;
    }
    public async Task RequestNavigateAsync(NavigationContext navigationContext, Action? onCompleted = null, bool coordinatePlacement = true)
    {
        navigationContext.ActivatedExternally = false;
        // Set up the indicator host before anything that could throw (job scheduling, lease
        // acquisition), so the catch block below can always show an error indicator - not just
        // when the navigation got far enough to reach CreateNavigateTask's own Setup() call.
        _regionIndicatorManager.Setup(navigationContext, _regionPresenter.IsSinglePageRegion);
        try
        {
            await _navigationJobScheduler.RunJobAsync(navigationContext, async context =>
            {
                using var flow = !coordinatePlacement || _placement is null ? null : _placement.EnsureFlow();
                using var lease = !coordinatePlacement || _placement is null ? null :
                    await _placement.EnterAsync(context.RegionName, context.CancellationToken);
                await CreateNavigateTask(context);
                onCompleted?.Invoke();
            }, _navigationJobStrategy);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (navigationContext.IndicatorHost.IsSet)
                await _regionIndicatorManager.ShowErrorAsync(navigationContext, ex);
            throw;
        }
    }

    public Task OnNavigateFromAsync(NavigationContext navigationContext)
    {
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        return OnBeforeNavigationAsync(navigationContext);
    }
    public Task RevertAsync(NavigationContext? navigationContext)
    {
        if (Current.HasValue && _regionPresenter.IsSinglePageRegion)
        {
            _regionPresenter.ProcessActivateAsync(Current.Value.NavigationContext);
            return _regionIndicatorManager.Revert(Current.Value.NavigationContext);
        }
        else
        {
            return _regionPresenter.ProcessDeactivateAsync(navigationContext);
        }
    }
    public void SetCurrent(NavigationContext? context) =>
        _current = context?.Target.Value is { } view ? (view, context) : null;

    public void DetachCurrent(IView? view)
    {
        if (Current.HasValue && ReferenceEquals(Current.Value.View, view))
            _current = null;
    }

    public void ForgetView(IView view)
    {
        DetachCurrent(view);
        (_viewCacheManager as IViewPlacementCache)?.RemoveInstance(view);
    }

    public Task PreparePlacementAsync(NavigationContext context) => OnBeforeNavigationAsync(context);
    public async Task CommitPlacementAsync(NavigationContext context, bool notify)
    {
        if (notify) await OnAfterNavigationAsync(context);
        else
        {
            SubscribeUnload(context);
            SetCurrent(context);
        }
    }

    public void Dispose()
    {
        try
        {
            _navigationJobScheduler.CancelAllAsync();
            _navigationJobScheduler.WaitAllAsync();
        }
        catch
        {
            //ignore
        }
    }

    private async Task CreateNavigateTask(NavigationContext navigationContext)
    {
        var isSinglePageRegion = _regionPresenter!.IsSinglePageRegion;
        // Set up the indicator host up front so it is always available - even if the
        // fast path below (cache lookup / TryActivateAsync) throws or short-circuits -
        // so a failed navigation can still show an error and a successful NavigationResult
        // always carries a usable IndicatorHost.
        _regionIndicatorManager.Setup(navigationContext, isSinglePageRegion);
        if (_placement is not null)
        {
            // Inspect reusable instances before leaving or rendering the current region.
            // New views still initialize in the normal pipeline, after the leave guard.
            if (!navigationContext.Target.IsSet && _regionPresenter.EnableViewCache &&
                _viewCacheManager is IViewPlacementCache cache)
            {
                var cached = await cache.FindCachedViewAsync(navigationContext.ViewName,
                    view => HandleIsNavigationTargetAsync(view, navigationContext));
                navigationContext.CacheLookupCompleted = true;
                if (cached is not null) navigationContext.Target.Value = cached;
            }
            if (navigationContext.Target.IsSet && navigationContext.Target.Value is { } target &&
                await _placement.TryActivateAsync(target, navigationContext.CancellationToken))
            {
                navigationContext.ActivatedExternally = true;
                return;
            }
        }

        var navigationTask = RunNavigationAsync(navigationContext, _regionPresenter.NavigationPipelineMode);

        await _regionIndicatorManager.StartAsync(
            navigationContext,
            navigationTask,
            NavigationOptions.Default.LoadingIndicatorDelay);
    }
    private Task RunNavigationAsync(NavigationContext context, NavigationPipelineMode mode)
    {
        Func<NavigationContext, Task>[] pipeline = mode switch
        {
            NavigationPipelineMode.RenderFirst =>
                [
                    OnRenderIndicatorAsync, 
                    OnBeforeNavigationAsync, 
                    OnResolveViewAsync, 
                    OnAfterNavigationAsync
                ],
            NavigationPipelineMode.ResolveFirst =>
                [
                    OnBeforeNavigationAsync, 
                    OnResolveViewAsync, 
                    OnRenderIndicatorAsync, 
                    OnAfterNavigationAsync
                ],
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

        return RegionNavigationService<T>.ExecutePipelineAsync(pipeline, context);
    }

    private Task OnRenderIndicatorAsync(NavigationContext navigationContext)
    {
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        _regionPresenter.ProcessActivateAsync(navigationContext);
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
    private async Task OnResolveViewAsync(NavigationContext navigationContext)
    {
        if (navigationContext.Target.IsSet)
        {
            return;
        }
        var view = await _viewCacheManager.ResolveViewAsync(navigationContext.ViewName,
                                _regionPresenter.EnableViewCache && !navigationContext.CacheLookupCompleted,
                                view => RegionNavigationService<T>.HandleIsNavigationTargetAsync(view, navigationContext),
                                view => RegionNavigationService<T>.HandleInitializeAsync(view, navigationContext));
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        navigationContext.Target.Value = view;
    }

    private async Task OnBeforeNavigationAsync(NavigationContext navigationContext)
    {
        if (Current.HasValue)
        {
            var currentDataContext = Current.Value.View.DataContext;

            // Check navigation guard before notifying the current view it is leaving.
            // If the guard blocks navigation, throw OperationCanceledException so the
            // pipeline treats this as a cancellation and reverts to the current view.
            if (currentDataContext is INavigationGuard guard)
            {
                var canNavigate = await guard.CanNavigateAsync(navigationContext, navigationContext.CancellationToken);
                navigationContext.CancellationToken.ThrowIfCancellationRequested();
                if (!canNavigate)
                    throw new OperationCanceledException("Navigation was blocked by INavigationGuard.", navigationContext.CancellationToken);
            }

            if (currentDataContext is INavigationAware currentAware)
            {
                await currentAware.OnNavigatedFromAsync(navigationContext);
            }
        }
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
    }

    private async Task OnAfterNavigationAsync(NavigationContext navigationContext)
    {
        if (navigationContext.TryResolveViewAndAware(out var view,out var aware))
        {
            SubscribeUnload(navigationContext);
            await aware.OnNavigatedToAsync(navigationContext);
            if (navigationContext.CancellationToken.IsCancellationRequested)
            {
                // Activation succeeded but the operation is being cancelled/rolled back right
                // after - notify the view it's leaving too, so its lifecycle state doesn't end
                // up out of sync with the region reverting to whatever was active before.
                await aware.OnNavigatedFromAsync(navigationContext);
                navigationContext.CancellationToken.ThrowIfCancellationRequested();
            }
            _current = (view, navigationContext);
        }
        if (navigationContext.Target.Value is { } target)
            _current = (target, navigationContext);
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
    }

    private void SubscribeUnload(NavigationContext navigationContext)
    {
        if (!navigationContext.TryResolveNavigationAware(out var aware)) return;
        var contextSnapshot = navigationContext;
        WeakUnloadObserver.Subscribe(aware,async a =>
        {
            if (Current.HasValue)
            {
                if (ReferenceEquals(Current.Value.View.DataContext, a))
                    _current = null;
            }

            await _regionPresenter.ProcessDeactivateAsync(contextSnapshot);
        });
    }

    private static Task<bool> HandleIsNavigationTargetAsync(IView view, NavigationContext navigationContext)
    {
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        if (view.DataContext is INavigationAware navigationAware)
        {
            return navigationAware.IsNavigationTargetAsync(navigationContext);
        }
        // todo: throw?
        return Task.FromResult(false);
    }
    private static Task HandleInitializeAsync(IView view, NavigationContext navigationContext)
    {
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        if (view.DataContext is INavigationAware navigationAware)
        {
            return navigationAware.InitializeAsync(navigationContext);
        }
        // todo: throw?
        return Task.CompletedTask;
    }

    private static async Task ExecutePipelineAsync(IEnumerable<Func<NavigationContext, Task>> pipelines, NavigationContext navigationContext)
    {
        Debug.WriteLine($"[{Environment.CurrentManagedThreadId}]Start:{navigationContext}");
        try
        {
            foreach (var pipeline in pipelines)
            {
                Debug.WriteLine($"[{Environment.CurrentManagedThreadId}]{navigationContext} # {pipeline.Method?.Name}");
                navigationContext.CancellationToken.ThrowIfCancellationRequested();
                await pipeline(navigationContext);
            }
        }
        finally
        {
            Debug.WriteLine($"[{Environment.CurrentManagedThreadId}]End:{navigationContext}");
        }
    }
}
