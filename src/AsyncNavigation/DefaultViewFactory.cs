using AsyncNavigation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AsyncNavigation;

internal sealed class DefaultViewFactory : IViewFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyList<ServiceDescriptor> _serviceDescriptors;
    private readonly ConcurrentDictionary<string, Func<IView>> _viewFactories = new();

    public DefaultViewFactory(IServiceProvider serviceProvider, IEnumerable<ServiceDescriptor> serviceDescriptors)
    {
        _serviceProvider = serviceProvider;
        _serviceDescriptors = [.. serviceDescriptors];
    }

    public IView CreateView(string viewName)
    {
        var factory = _viewFactories.GetOrAdd(viewName, CreateViewFactory);
        var view = factory();
        Debug.WriteLine($"Created view {viewName} of type {view.GetType().Name}");
        return view;
    }

    public void AddView(string key, IView view)
    {
        if(_viewFactories.TryGetValue(key, out _))
            throw new ArgumentException($"View with key '{key}' already exists.");
        _viewFactories.TryAdd(key, () => view);
    }
    public void AddView(string key, Func<string, IView> viewBuilder)
    {
        if (_viewFactories.TryGetValue(key, out _))
            throw new ArgumentException($"View with key '{key}' already exists.");
        _viewFactories.TryAdd(key, () => viewBuilder.Invoke(key));
    }

    public bool CanCreateView(string viewName)
    {
        return _serviceDescriptors.Any(sd =>
            sd.ServiceType == typeof(IView) &&
            sd.ServiceKey?.Equals(viewName) == true);
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
                    view.DataContext = _serviceProvider.GetRequiredKeyedService<INavigationAware>(viewName);
                }
                return view;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw new InvalidOperationException($"Failed to create view for '{viewName}'", ex);
            }
        };
    }
}
