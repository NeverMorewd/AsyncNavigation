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
    }
    public async Task RequestNavigateAsync(NavigationContext navigationContext)
    {
        try
        {
            await _navigationJobScheduler.RunJobAsync(navigationContext, CreateNavigateTask);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
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
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        return OnBeforeNavigationAsync(navigationContext);
    }
    public Task RevertAsync(NavigationContext? navigationContext)
    {
        if (Current.HasValue && _regionPresenter.IsSinglePageRegion)
        {
            _regionPresenter.ProcessActivate(Current.Value.NavigationContext);
            return _regionIndicatorManager.Revert(Current.Value.NavigationContext);
        }
        else
        {
            _regionPresenter.ProcessDeactivate(navigationContext);
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
                    OnResovleViewAsync, 
                    OnAfterNavigationAsync
                ],
            NavigationPipelineMode.ResolveFirst =>
                [
                    OnBeforeNavigationAsync, 
                    OnResovleViewAsync, 
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
        _regionPresenter.ProcessActivate(navigationContext);
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
    private async Task OnResovleViewAsync(NavigationContext navigationContext)
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

    private async Task OnBeforeNavigationAsync(NavigationContext navigationContext)
    {
        if (Current.HasValue)
        {
            var currentAware = (Current.Value.View.DataContext as INavigationAware)!;
            await currentAware.OnNavigatedFromAsync(navigationContext);
            // todo: why detach?
            //if (_regionPresenter.IsSinglePageRegion)
            //{
            //    _unloadHandler.Detach(currentAware);
            //}
        }
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
    }

    private async Task OnAfterNavigationAsync(NavigationContext navigationContext)
    {
        if (navigationContext.Target.Value is IView view
            && view.DataContext is INavigationAware aware)
        {
            var contextSnapshot = navigationContext;
            WeakUnloadObserver.Subscribe(aware, a =>
            {
                if (Current.HasValue)
                {
                    if (ReferenceEquals(Current.Value.View.DataContext, a))
                        _current = null;
                }

                _regionPresenter.ProcessDeactivate(contextSnapshot);
            });
            await aware.OnNavigatedToAsync(navigationContext);
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
            _current = (view, navigationContext);
        }
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
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
