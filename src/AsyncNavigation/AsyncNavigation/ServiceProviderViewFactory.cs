using AsyncNavigation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AsyncNavigation;

public class ServiceProviderViewFactory : IViewFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyList<ServiceDescriptor> _serviceDescriptors;
    private readonly ConcurrentDictionary<string, Func<IView>> _viewFactories = new();

    public ServiceProviderViewFactory(IServiceProvider serviceProvider, IEnumerable<ServiceDescriptor> serviceDescriptors)
    {
        _serviceProvider = serviceProvider;
        _serviceDescriptors = [.. serviceDescriptors];
    }

    public async Task<IView> CreateViewAsync(string viewName, CancellationToken cancellationToken = default)
    {
        var factory = _viewFactories.GetOrAdd(viewName, CreateViewFactory);
        cancellationToken.ThrowIfCancellationRequested();
        var view = factory();
        Debug.WriteLine($"Created view {viewName} of type {view.GetType().Name}");
        return await Task.FromResult(view);
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
                if (_serviceProvider.GetKeyedService<INavigationAware>(viewName) is { } vm)
                    view.DataContext = vm;
                return view;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create view for '{viewName}'", ex);
            }
        };
    }

}
