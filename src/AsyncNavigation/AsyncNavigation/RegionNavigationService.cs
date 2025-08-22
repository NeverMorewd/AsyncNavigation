using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace AsyncNavigation;

internal sealed class RegionNavigationService<T> : IRegionNavigationService<T> where T : IRegionPresenter
{
    private readonly AsyncConcurrentItem<IView> Current = new();
    private readonly IViewManager _viewCacheManager;
    private readonly IRegionIndicatorManager _regionIndicatorManager;
    private readonly INavigationJobScheduler _navigationTaskManager;
    private readonly IRegionPresenter _regionPresenter;
    private readonly RequestUnloadHandler _unloadHandler;

    public RegionNavigationService(T regionPresenter, IServiceProvider serviceProvider)
    {
        _regionPresenter = regionPresenter;
        _navigationTaskManager = serviceProvider.GetRequiredService<INavigationJobScheduler>();
        _viewCacheManager = serviceProvider.GetRequiredService<IViewManager>();
        _regionIndicatorManager = serviceProvider.GetRequiredService<IRegionIndicatorManager>();
        _unloadHandler = new RequestUnloadHandler(_regionPresenter, _viewCacheManager);
    }
    public async Task<NavigationResult> RequestNavigateAsync(NavigationContext navigationContext)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _navigationTaskManager.RunJobAsync(navigationContext, CreateNavigateTask);
            return NavigationResult.Success(stopwatch.Elapsed);
        }
        catch (OperationCanceledException ocex) when (navigationContext.CancellationToken.IsCancellationRequested)
        {
            await _regionIndicatorManager.ShowErrorAsync(navigationContext, ocex);
            return NavigationResult.Cancelled(stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            await _regionIndicatorManager.ShowErrorAsync(navigationContext, ex);
            return NavigationResult.Failure(ex, stopwatch.Elapsed);
        }
        finally
        {
            stopwatch.Stop();
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
    }

    private Task RunSinglePageNavigationAsync(NavigationContext navigationContext)
    {
        Func<NavigationContext, Task>[] steps = 
            [RenderIndicatorAsync,
             HandleBeforeNavigationAsync, 
             ResovleViewAsync, 
             HandleAfterNavigationAsync];
        return RegionNavigationService<T>.ExecuteStepsAsync(steps, navigationContext);
    }

    private Task RunMultiPageNavigationAsync(NavigationContext navigationContext)
    {
        Func<NavigationContext, Task>[] steps = [HandleBeforeNavigationAsync, ResovleViewAsync, RenderIndicatorAsync, HandleAfterNavigationAsync];

        return RegionNavigationService<T>.ExecuteStepsAsync(steps, navigationContext);
    }

    private Task RenderIndicatorAsync(NavigationContext navigationContext)
    {
        _regionPresenter.RenderIndicator(navigationContext);
        return Task.CompletedTask;
    }
    private async Task ResovleViewAsync(NavigationContext navigationContext)
    {
        var view = await _viewCacheManager.ResolveViewAsync(navigationContext.ViewName,
                                _regionPresenter.EnableViewCache,
                                navigationContext);
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        navigationContext.Target.Value = view;
    }

    private async Task HandleBeforeNavigationAsync(NavigationContext navigationContext)
    {
        if (Current.TryTakeData(out IView? currentView))
        {
            var currentAware = (currentView!.DataContext as INavigationAware)!;
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
            await currentAware.OnNavigatedFromAsync(navigationContext);
            if (_regionPresenter.IsSinglePageRegion)
            {
                _unloadHandler.Detach(currentAware);
            }
        }
    }

    private async Task HandleAfterNavigationAsync(NavigationContext navigationContext)
    {
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        if (navigationContext.Target.Value is IView view
            && view.DataContext is INavigationAware aware)
        {
            _unloadHandler.Attach(aware, navigationContext);
            await aware.OnNavigatedToAsync(navigationContext);
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
            Current.SetData(view);
        }
    }


    private static async Task ExecuteStepsAsync(IEnumerable<Func<NavigationContext, Task>> steps, NavigationContext navigationContext)
    {
        foreach (var step in steps)
        {
            await step(navigationContext);
        }
    }
}
