using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace AsyncNavigation;

internal sealed class RegionNavigationService<T> : IRegionNavigationService<T> where T : IRegionPresenter
{
    private readonly IViewManager _viewCacheManager;
    private readonly IRegionIndicatorManager _regionIndicatorManager;
    private readonly IJobScheduler _navigationJobScheduler;
    private readonly IRegionPresenter _regionPresenter;
    private (IView View, NavigationContext NavigationContext)? _current;
    public RegionNavigationService(T regionPresenter, IServiceProvider serviceProvider)
    {
        _regionPresenter = regionPresenter;
        _navigationJobScheduler = serviceProvider.GetRequiredService<IJobScheduler>();
        _viewCacheManager = serviceProvider.GetRequiredService<IViewManager>();
        _regionIndicatorManager = serviceProvider.GetRequiredService<IRegionIndicatorManager>();
    }
    internal (IView View, NavigationContext NavigationContext)? Current
    {
        get => _current;
        set => _current = value;
    }
    public async Task RequestNavigateAsync(NavigationContext navigationContext)
    {
        try
        {
            await _navigationJobScheduler.RunJobAsync(navigationContext, CreateNavigateTask);
        }
        catch (OperationCanceledException) when (navigationContext.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _regionIndicatorManager.ShowErrorAsync(navigationContext, ex);
            throw;
        }
        finally
        {
#if GC_TEST
            GC.Collect();
#endif
        }
    }
    public Task OnNavigateFromAsync(NavigationContext navigationContext)
    {
        return HandleBeforeNavigationAsync(navigationContext);
    }
    public Task RevertAsync()
    {
        if (Current.HasValue)
        {
            _regionPresenter.ProcessActivate(Current.Value.NavigationContext);
            return _regionIndicatorManager.Revert(Current.Value.NavigationContext);
        }
        else
        {
            _regionPresenter.ProcessDeactivate(null);
            return Task.CompletedTask;
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
        _regionIndicatorManager.Setup(navigationContext, isSinglePageRegion);

        var navigationTask = isSinglePageRegion
            ? RunSinglePageNavigationAsync(navigationContext)
            : RunMultiPageNavigationAsync(navigationContext);

        await _regionIndicatorManager.StartAsync(
            navigationContext,
            navigationTask,
            NavigationOptions.Default.LoadingIndicatorDelay);

        //_regionPresenter.ProcessActivate(navigationContext);
    }

    private Task RunSinglePageNavigationAsync(NavigationContext navigationContext)
    {
        Func<NavigationContext, Task>[] steps =
            [
             RenderIndicatorAsync,
             HandleBeforeNavigationAsync,
             ResovleViewAsync,
             HandleAfterNavigationAsync
            ];
        return RegionNavigationService<T>.ExecuteStepsAsync(steps, navigationContext);
    }

    private Task RunMultiPageNavigationAsync(NavigationContext navigationContext)
    {
        Func<NavigationContext, Task>[] steps = 
            [
             HandleBeforeNavigationAsync, 
             ResovleViewAsync, 
             RenderIndicatorAsync, 
             HandleAfterNavigationAsync
            ];
        return RegionNavigationService<T>.ExecuteStepsAsync(steps, navigationContext);
    }

    private Task RenderIndicatorAsync(NavigationContext navigationContext)
    {
        _regionPresenter.ProcessActivate(navigationContext);
        return Task.CompletedTask;
    }
    private async Task ResovleViewAsync(NavigationContext navigationContext)
    {
        if (navigationContext.Target.IsSet)
        {
            return;
        }
        var view = await _viewCacheManager.ResolveViewAsync(navigationContext.ViewName,
                                _regionPresenter.EnableViewCache,
                                view => RegionNavigationService<T>.HandleIsNavigationTargetAsync(view, navigationContext),
                                view => RegionNavigationService<T>.HandleInitializeAsync(view, navigationContext));
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        navigationContext.Target.Value = view;
    }

    private async Task HandleBeforeNavigationAsync(NavigationContext navigationContext)
    {
        if (Current.HasValue)
        {
            var currentAware = (Current.Value.View.DataContext as INavigationAware)!;
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
            await currentAware.OnNavigatedFromAsync(navigationContext);

            // todo: why detach?
            //if (_regionPresenter.IsSinglePageRegion)
            //{
            //    _unloadHandler.Detach(currentAware);
            //}
        }
    }

    private async Task HandleAfterNavigationAsync(NavigationContext navigationContext)
    {
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        if (navigationContext.Target.Value is IView view
            && view.DataContext is INavigationAware aware)
        {
            var contextSnapshot = navigationContext;
            WeakUnloadObserver.Subscribe(aware, a =>
            {
                if (Current.HasValue)
                {
                    if (ReferenceEquals(Current.Value.View.DataContext, a))
                        Current = null;
                }

                _regionPresenter.ProcessDeactivate(contextSnapshot);
            });
            await aware.OnNavigatedToAsync(navigationContext);
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
            Current = (view, navigationContext);
        }
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

    private static async Task ExecuteStepsAsync(IEnumerable<Func<NavigationContext, Task>> steps, NavigationContext navigationContext)
    {
        foreach (var step in steps)
        {
            await step(navigationContext);
        }
    }
}
