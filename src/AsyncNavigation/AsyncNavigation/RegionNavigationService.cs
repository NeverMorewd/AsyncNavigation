using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace AsyncNavigation;

public class RegionNavigationService<T> : IRegionNavigationService<T> where T : IRegionProcessor
{
    private readonly AsyncConcurrentItem<IView> Current = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IViewCacheManager  _viewCacheManager;
    private readonly IViewFactory _viewFactory;
    private readonly IRegionIndicatorManager _regionIndicatorManager;
    private readonly INavigationTaskManager _navigationTaskManager;

    private IRegionProcessor? _regionProcessor;

    public RegionNavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _navigationTaskManager = _serviceProvider.GetRequiredService<INavigationTaskManager>();
        _viewCacheManager = _serviceProvider.GetRequiredService<IViewCacheManager>();
        _viewFactory = _serviceProvider.GetRequiredService<IViewFactory>();
        _regionIndicatorManager = _serviceProvider.GetRequiredService<IRegionIndicatorManager>();
    }
    public void SeRegionProcessor(T regionProcessor)
    {
        _regionProcessor = regionProcessor;
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
        var indicator = _regionIndicatorManager.Setup(navigationContext, _regionProcessor!.IsSinglePageRegion);
        _regionProcessor!.RenderIndicator(navigationContext, indicator);
        await _regionIndicatorManager.StartAsync(navigationContext, StartProcessNavigation(navigationContext), NavigationOptions.Default.LoadingIndicatorDelay);
    }

    private async Task ResovleViewAsync(NavigationContext navigationContext)
    {
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        
        if (_regionProcessor!.EnableViewCache)
        {
            if (_viewCacheManager.TryCachedView(navigationContext.ViewName, out var cacheView))
            {
                var cacheAware = (cacheView!.DataContext as INavigationAware)!;
                if (await cacheAware.IsNavigationTargetAsync(navigationContext, navigationContext.CancellationToken))
                {
                    navigationContext.CancellationToken.ThrowIfCancellationRequested();
                    navigationContext.Target.Value = cacheView;
                    return;
                }
            }
        }

        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        
        var view = _viewFactory.CreateView(navigationContext.ViewName);

        if (view.DataContext is INavigationAware aware)
        {
            await aware.InitializeAsync(navigationContext.CancellationToken);
        }

        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        
        navigationContext.Target.Value = view;
        await _viewCacheManager.SetCachedViewAsync(navigationContext.ViewName, (view as IView)!);
    }

    private async Task HandleBeforeNavigationAsync(NavigationContext navigationContext)
    {
        if (Current.TryTakeData(out IView? currentView))
        {
            var currentAware = (currentView!.DataContext as INavigationAware)!;
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
            await currentAware.OnNavigatedFromAsync(navigationContext, navigationContext.CancellationToken);
        }
    }

    private async Task HandleAfterNavigationAsync(NavigationContext navigationContext)
    {
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        if (navigationContext.Target.Value is IView view
            && view.DataContext is INavigationAware aware)
        {
            await aware.OnNavigatedToAsync(navigationContext, navigationContext.CancellationToken);
            Current.SetData(view);
        }
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
    }
    
    private async Task StartProcessNavigation(NavigationContext navigationContext)
    {
        await HandleBeforeNavigationAsync(navigationContext);
        await ResovleViewAsync(navigationContext);
        await HandleAfterNavigationAsync(navigationContext);
        //await _regionIndicatorManager.ShowContentAsync(navigationContext, navigationContext.Target.Value!);
    }
}
