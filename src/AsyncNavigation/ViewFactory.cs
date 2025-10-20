using AsyncNavigation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AsyncNavigation;

internal sealed class ViewFactory : IViewFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, Func<IView>> _viewFactories = new();

    public ViewFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IView CreateView(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
            throw new ArgumentException("View name cannot be null or whitespace.", nameof(viewName));

        var factory = _viewFactories.GetOrAdd(viewName, CreateViewFactory);
        var view = factory();

        Debug.WriteLine($"Created view {viewName} of type {view.GetType().Name}");
        return view;
    }

    public void AddView(string key, IView view)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("View key cannot be null or whitespace.", nameof(key));

        if (!_viewFactories.TryAdd(key, () => view))
            throw new ArgumentException($"View with key '{key}' already exists.");
    }

    public void AddView(string key, Func<string, IView> viewBuilder)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("View key cannot be null or whitespace.", nameof(key));

        if (!_viewFactories.TryAdd(key, () => viewBuilder(key)))
            throw new ArgumentException($"View with key '{key}' already exists.");
    }

    public bool CanCreateView(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
            return false;

        return _serviceProvider.GetKeyedService<IView>(viewName) is not null;
    }

    private Func<IView> CreateViewFactory(string viewName)
    {
        return () =>
        {
            try
            {
                var view = _serviceProvider.GetRequiredKeyedService<IView>(viewName);

                if (view.DataContext is not INavigationAware)
                {
                    var navigationAware = _serviceProvider.GetKeyedService<INavigationAware>(viewName);
                    if (navigationAware is not null)
                        view.DataContext = navigationAware;
                }

                return view;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create view for '{viewName}': {ex}");
                throw new InvalidOperationException($"Failed to create view for '{viewName}'", ex);
            }
        };
    }
}
