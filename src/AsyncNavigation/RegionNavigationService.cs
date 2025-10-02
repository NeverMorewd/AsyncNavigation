using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace AsyncNavigation;

internal sealed class RegionNavigationService<T> : IRegionNavigationService<T> where T : IRegionPresenter
{
    private readonly IViewManager _viewCacheManager;
    private readonly IRegionIndicatorManager _regionIndicatorManager;
    private readonly INavigationJobScheduler _navigationJobScheduler;
    private readonly IRegionPresenter _regionPresenter;
    private readonly WeakRequestUnloadHandler _unloadHandler;
    private volatile IView? _current;
    public RegionNavigationService(T regionPresenter, IServiceProvider serviceProvider)
    {
        _regionPresenter = regionPresenter;
        _navigationJobScheduler = serviceProvider.GetRequiredService<INavigationJobScheduler>();
        _viewCacheManager = serviceProvider.GetRequiredService<IViewManager>();
        _regionIndicatorManager = serviceProvider.GetRequiredService<IRegionIndicatorManager>();
        _unloadHandler = new WeakRequestUnloadHandler(_regionPresenter,
            _viewCacheManager,
            aware =>
            {
                if (CurrentView != null && CurrentView.DataContext == aware)
                {
                    CurrentView = null;
                }
            });
    }
    internal IView? CurrentView
    {
        get => _current;
        set => _current = value;
    }
    public async Task<NavigationResult> RequestNavigateAsync(NavigationContext navigationContext)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _navigationJobScheduler.RunJobAsync(navigationContext, CreateNavigateTask);
            return NavigationResult.Success(stopwatch.Elapsed, navigationContext);
        }
        catch (OperationCanceledException) when (navigationContext.CancellationToken.IsCancellationRequested)
        {
            return NavigationResult.Cancelled(stopwatch.Elapsed, navigationContext);
        }
        catch (Exception ex)
        {
            var result = NavigationResult.Failure(ex, stopwatch.Elapsed, navigationContext);
            await _regionIndicatorManager.ShowErrorAsync(navigationContext, ex);
            return result;
        }
        finally
        {
            stopwatch.Stop();

#if GC_TEST
            GC.Collect();
#endif
        }
    }
    public Task OnNavigateFromAsync(NavigationContext navigationContext)
    {
        return HandleBeforeNavigationAsync(navigationContext);
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

        _regionPresenter.ProcessActivate(navigationContext);
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
        if (navigationContext.Target.IsSet)
        {
            return;
        }
        var view = await _viewCacheManager.ResolveViewAsync(navigationContext.ViewName,
                                _regionPresenter.EnableViewCache,
                                view => HandleIsNavigationTargetAsync(view, navigationContext),
                                view => HandleInitializeAsync(view, navigationContext));
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        navigationContext.Target.Value = view;
    }

    private async Task HandleBeforeNavigationAsync(NavigationContext navigationContext)
    {
        if (CurrentView is not null)
        {
            var currentAware = (CurrentView.DataContext as INavigationAware)!;
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
            _unloadHandler.Attach(aware, navigationContext);
            await aware.OnNavigatedToAsync(navigationContext);
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
            CurrentView = view;
        }
    }

    private Task<bool> HandleIsNavigationTargetAsync(IView view, NavigationContext navigationContext)
    {
        if (view.DataContext is INavigationAware navigationAware)
        {
            return navigationAware.IsNavigationTargetAsync(navigationContext);
        }
        // todo: throw?
        return Task.FromResult(true);
    }
    private Task HandleInitializeAsync(IView view, NavigationContext navigationContext)
    {
        if (view.DataContext is INavigationAware navigationAware)
        {
            return navigationAware.InitializeAsync(navigationContext);
        }
        // todo: throw?
        return Task.FromResult(true);
    }

    private static async Task ExecuteStepsAsync(IEnumerable<Func<NavigationContext, Task>> steps, NavigationContext navigationContext)
    {
        foreach (var step in steps)
        {
            await step(navigationContext);
        }
    }
}
