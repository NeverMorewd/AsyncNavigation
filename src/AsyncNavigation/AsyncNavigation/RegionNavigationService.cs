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
    private readonly INavigationTaskManager _navigationTaskManager;
    private readonly IRegionPresenter _regionPresenter;
    private readonly RequestUnloadHandler _unloadHandler;

    public RegionNavigationService(T regionPresenter, IServiceProvider serviceProvider)
    {
        _regionPresenter = regionPresenter;
        _navigationTaskManager = serviceProvider.GetRequiredService<INavigationTaskManager>();
        _viewCacheManager = serviceProvider.GetRequiredService<IViewManager>();
        _regionIndicatorManager = serviceProvider.GetRequiredService<IRegionIndicatorManager>();
        _unloadHandler = new RequestUnloadHandler(_regionPresenter, _viewCacheManager);
    }
    public async Task<NavigationResult> RequestNavigateAsync(NavigationContext navigationContext)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _navigationTaskManager.StartNavigationAsync(navigationContext, CreateNavigateTask);
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
        var indicator = _regionIndicatorManager.Setup(navigationContext, _regionPresenter!.IsSinglePageRegion);
        _regionPresenter.RenderIndicator(navigationContext, indicator);
        await _regionIndicatorManager.StartAsync(navigationContext,
            StartProcessNavigation(navigationContext),
            NavigationOptions.Default.LoadingIndicatorDelay);
    }

    private async Task StartProcessNavigation(NavigationContext navigationContext)
    {
        await HandleBeforeNavigationAsync(navigationContext);
        await ResovleViewAsync(navigationContext);
        await HandleAfterNavigationAsync(navigationContext);
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
            await currentAware.OnNavigatedFromAsync(navigationContext, navigationContext.CancellationToken);
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
            await aware.OnNavigatedToAsync(navigationContext, navigationContext.CancellationToken);
            Current.SetData(view);
        }
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
    }
}
