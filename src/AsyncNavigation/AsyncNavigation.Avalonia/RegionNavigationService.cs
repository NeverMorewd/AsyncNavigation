using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Logging;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AsyncNavigation.Avalonia;

public class RegionNavigationService<T> : IRegionNavigationService<T> where T : IRegionProcessor
{
    private readonly ConcurrentDictionary<NavigationContext, NavigationTaskFacade> _taskFacades = new();
    private readonly AsyncConcurrentItem<IView> Current = new();
    private readonly ConcurrentDictionary<string, IDataTemplate> _availableViewTemplates = [];
    private readonly IServiceProvider _serviceProvider;
    private IRegionProcessor? _regionProcessor;

    private readonly IViewCacheManager  _viewCacheManager;
    private readonly IViewFactory _viewFactory;
    private readonly IRegionIndicatorManager<ContentControl> _regionIndicatorManager;


    public RegionNavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _viewCacheManager = _serviceProvider.GetRequiredService<IViewCacheManager>();
        _viewFactory = _serviceProvider.GetRequiredService<IViewFactory>();
        _regionIndicatorManager = _serviceProvider.GetRequiredService<IRegionIndicatorManager<ContentControl>>();
    }
    public void SeRegionProcessor(T regionProcessor)
    {
        _regionProcessor = regionProcessor;
    }
    
    public async Task<NavigationResult> RequestNavigateAsync(NavigationContext navigationContext)
    {
        using var reg = navigationContext.CancellationToken.Register(() =>
        {
            navigationContext.WithStatus(NavigationStatus.Cancelled);
        });
        var stopwatch = Stopwatch.StartNew();
        try
        {
            navigationContext.WithStatus(NavigationStatus.InProgress);
            if (_regionProcessor!.IsSinglePageRegion)
            {
                _regionIndicatorManager.SetupSingletonIndicator(navigationContext);
            }
            else
            {
                _regionIndicatorManager.SetupIndicator(navigationContext);
            }
            _regionProcessor!.ProcessActivate(navigationContext);
            var processTask = StartProcessNavigation(navigationContext);
            await _regionIndicatorManager.DelayShowLoadingAsync(navigationContext, processTask, navigationContext.CancellationToken);
            await processTask;
            stopwatch.Stop();
            navigationContext.WithStatus(NavigationStatus.Succeeded);
            return NavigationResult.Success(stopwatch.Elapsed);
        }
        catch (OperationCanceledException cex)
        {
            stopwatch.Stop();
            await _regionIndicatorManager.ShowErrorAsync(navigationContext, cex);
            return NavigationResult.Cancelled(stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await _regionIndicatorManager.ShowErrorAsync(navigationContext, ex);
            return NavigationResult.Failure(ex, stopwatch.Elapsed);
        }
        finally
        {
            await WaitNavigationAsync(navigationContext);
        }
    }
    
    private async Task WaitNavigationAsync(NavigationContext navigationContext)
    {
        if (_taskFacades.TryRemove(navigationContext, out var taskFacade))
        {
            await taskFacade;
        }
    }

    private async Task ResovleViewAsync(NavigationContext navigationContext)
    {
        try
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


            var template = _availableViewTemplates.GetOrAdd(navigationContext.ViewName, name =>
            {
                return new FuncDataTemplate<NavigationContext>((context, np) =>
                {
                    try
                    {
                        var view = _serviceProvider.GetRequiredKeyedService<IView>(name);
                        var vm = _serviceProvider.GetRequiredKeyedService<INavigationAware>(name);
                        view.DataContext = vm;
                        return view as Control;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to create view for '{name}'", ex);
                    }
                }, true);
            });

            var view = template.Build(navigationContext) ?? throw new InvalidOperationException($"Template for view '{navigationContext.ViewName}' returned null");
            navigationContext.CancellationToken.ThrowIfCancellationRequested();
            navigationContext.Target.Value = view;

            if (view.DataContext is INavigationAware aware)
            {
                await aware.InitializeAsync(navigationContext.CancellationToken);
            }
            if (_regionProcessor!.EnableViewCache)
            {
                await _viewCacheManager.SetCachedViewAsync(navigationContext.ViewName, (view as IView)!);
            }
        }
        catch (Exception ex)
        {
            navigationContext.WithErrors(ex);
        }
    }

    private async Task HandleBeforeNavigationAsync(NavigationContext navigationContext)
    {
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
        if (Current.TryTakeData(out IView? currentView))
        {
            var currentAware = (currentView!.DataContext as INavigationAware)!;
            await currentAware.OnNavigatedFromAsync(navigationContext, navigationContext.CancellationToken);
        }
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
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
        var precedingTask = HandleBeforeNavigationAsync(navigationContext);
        var resolveViewTask = ResovleViewAsync(navigationContext);
        var remainingTask = Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                return HandleAfterNavigationAsync(navigationContext);
            }
            finally
            {
                _taskFacades.TryRemove(navigationContext, out _);
            }
        }, DispatcherPriority.Background);
        var tasks = new NavigationTaskFacade(precedingTask, resolveViewTask, remainingTask, navigationContext);
        _taskFacades[navigationContext] = tasks;
        await tasks;
        await _regionIndicatorManager.ShowContentAsync(navigationContext, navigationContext.Target.Value!, navigationContext.CancellationToken);
    }

    private async Task WaitAllNavigationsAsync()
    {
        if (!_taskFacades.IsEmpty)
        {
            await Task.WhenAll(_taskFacades.Values.Select(v => v.WaitDefault()));
        }
    }

    private void CancelCurrentTasks()
    {
        _taskFacades.Clear();
    }
}
