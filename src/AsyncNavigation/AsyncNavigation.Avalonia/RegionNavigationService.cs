using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AsyncNavigation.Avalonia;

public class RegionNavigationService<T> : IRegionNavigationService<T> where T : IRegionProcessor
{
    private readonly ConcurrentDictionary<NavigationContext, NavigationTaskFacade> _taskFacades = new();
    private readonly ConcurrentDictionary<string, IView> _viewCache = [];
    private readonly object _cacheLock = new();
    private readonly AsyncConcurrentItem<IView> Current = new();
    private readonly ConcurrentDictionary<string, IDataTemplate> _availableViewTemplates = [];
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataTemplate? _loadingTemplate;
    private readonly IDataTemplate? _errorTemplate;
    private ContentControl? _indicatorContainer;
    private IRegionProcessor? _regionProcessor;
    private readonly ConcurrentDictionary<NavigationContext, ContentControl> _indicatorContainers = [];

    public RegionNavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        if (AsyncNavigationOptions.EnableLoadingIndicator)
        {
            _loadingTemplate = _serviceProvider.GetKeyedService<IDataTemplate>(AsyncNavigationConstants.INDICATOR_LOADING_KEY);
        }
        if (AsyncNavigationOptions.EnableLoadingIndicator)
        {
            _errorTemplate = _serviceProvider.GetKeyedService<IDataTemplate>(AsyncNavigationConstants.INDICATOR_ERROR_KEY);
        }
    }

    public IObservable<NavigationContext?> Navigated => throw new NotImplementedException();

    public IObservable<NavigationContext?> Navigating => throw new NotImplementedException();

    public IObservable<NavigationContext?> NavigationFailed => throw new NotImplementedException();

    public async Task RequestNavigateAsync(NavigationContext navigationContext)
    {
        using var reg = navigationContext.CancellationToken.Register(() =>
        {
            navigationContext.WithStatus(NavigationStatus.Cancelled);
        });
        navigationContext.WithStatus(NavigationStatus.InProgress);
        try
        {
            if (AsyncNavigationOptions.EnableLoadingIndicator)
            {
                if (_regionProcessor!.AllowMultipleViews)
                {
                    var indicator = _indicatorContainers.GetOrAdd(navigationContext, _ => new ContentControl());
                    navigationContext.Indicator.Value = indicator;
                }
                else
                {
                    _indicatorContainer ??= new ContentControl();
                    navigationContext.Indicator.Value = _indicatorContainer;
                }
                var processTask = StartProcessNavigation(navigationContext);
                await Task.WhenAny(processTask, Task.Delay(AsyncNavigationOptions.LoadingDisplayDelay, navigationContext.CancellationToken));
                if (!processTask.IsCompleted)
                {
                    ((ContentControl)navigationContext.Indicator.Value).Content = _loadingTemplate!.Build(navigationContext);
                }
                await processTask;
            }
            else
            {
                await StartProcessNavigation(navigationContext);
                navigationContext.Indicator.Value = navigationContext.Target.Value;
            }
            _regionProcessor!.ProcessActivate(navigationContext);
            await WaitNavigationAsync(navigationContext);
            navigationContext.WithStatus(NavigationStatus.Succeeded);
        }
        catch (Exception ex)
        {
            if (AsyncNavigationOptions.EnableErrorIndicator)
            {
                if (navigationContext.Indicator.IsSet
                    && navigationContext.Indicator.Value is ContentControl indicatorContainer)
                {
                    indicatorContainer!.Content = _errorTemplate!.Build(navigationContext.WithStatus(NavigationStatus.Failed, ex));
                }
            }
            throw;
        }
    }
    public async Task WaitNavigationAsync(NavigationContext navigationContext)
    {
        if (_taskFacades.TryRemove(navigationContext, out var taskFacade))
        {
            await taskFacade;
        }
    }
    public async Task WaitAllNavigationsAsync()
    {
        if (!_taskFacades.IsEmpty)
        {
            await Task.WhenAll(_taskFacades.Values.Select(v => v.WaitDefault()));
        }
    }
    public void CancelCurrentTasks()
    {
        _taskFacades.Clear();
    }

    public void Setup(T regionProcessor)
    {
        _regionProcessor = regionProcessor;
    }

    public void AddView(string viewName, IView view)
    {
        _viewCache.AddOrUpdate(viewName,
                (name) => view,
                (name, oldView) => view);
    }
    public void AddView(string viewName)
    {
        if (!_availableViewTemplates.TryGetValue(viewName, out _))
        {
            _availableViewTemplates.TryAdd(viewName, new FuncDataTemplate<NavigationContext>((context, np) =>
            {
                try
                {
                    var view = _serviceProvider.GetRequiredKeyedService<IView>(viewName);
                    var vm = _serviceProvider.GetRequiredKeyedService<INavigationAware>(viewName);
                    view.DataContext = vm;
                    return view as Control;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to create view for '{viewName}'", ex);
                }
            }, true));
        }
    }


    private async Task ResovleViewAsync(NavigationContext navigationContext)
    {
        try
        {
            navigationContext.CancellationToken.ThrowIfCancellationRequested();

            if (_regionProcessor!.ShouldCheckSameNameViewCache)
            {
                if (_viewCache.TryGetValue(navigationContext.ViewName, out var cacheView))
                {
                    var cacheAware = (cacheView.DataContext as INavigationAware)!;
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
            if (_regionProcessor!.ShouldCheckSameNameViewCache)
            {
                //_viewCache.TryAdd(navigationContext.ViewName, (view as IView)!);
                _viewCache.AddOrUpdate(navigationContext.ViewName,
                    (name) => (view as IView)!,
                    (name, oldView) => (view as IView)!);
            }
        }
        catch (Exception ex)
        {
            navigationContext.WithErrors(ex);
        }
        navigationContext.CancellationToken.ThrowIfCancellationRequested();
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

    public bool RemoveFromCache(string viewName)
    {
        if (string.IsNullOrEmpty(viewName))
            return false;

        lock (_cacheLock)
        {
            if (_viewCache.TryRemove(viewName, out var view))
            {
                if (view is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                return true;
            }
        }
        return false;
    }

    public void ClearCache()
    {
        lock (_cacheLock)
        {
            foreach (var view in _viewCache.Values)
            {
                if (view is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _viewCache.Clear();
        }
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
        if (navigationContext.Indicator.Value != null 
            && navigationContext.Indicator.Value is ContentControl container)
        {
            container.Content = navigationContext.Target.Value;
        }
    }
}
