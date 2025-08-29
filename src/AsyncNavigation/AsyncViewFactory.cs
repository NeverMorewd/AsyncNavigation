using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncNavigation;

public class AsyncViewFactory : IAsyncViewFactory
{
    private readonly IServiceProvider _serviceProvider;

    public AsyncViewFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
    public IView CreateView(string viewName)
    {
        return _serviceProvider.GetRequiredKeyedService<IView>(viewName);
    }
    public async Task<IView> CreateViewAsync(string viewName, 
        NavigationContext context, 
        CancellationToken cancellationToken = default)
    {
        var view = _serviceProvider.GetRequiredKeyedService<IView>(viewName);
        var viewModel = _serviceProvider.GetRequiredKeyedService<INavigationAware>(viewName);
        view.DataContext = viewModel;
        await viewModel.InitializeAsync(cancellationToken);
        return view;
    }
}
