using AsyncNavigation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
namespace AsyncNavigation;
public class ViewContext
{
    private readonly IServiceProvider? _serviceProvider;
    private IView? _view;
    private INavigationAware? _viewModel;

    public ViewContext(string name, IServiceProvider serviceProvider)
    {
        ViewName = name ?? throw new ArgumentNullException(nameof(name));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public ViewContext(string name, IView view, INavigationAware viewModel)
    {
        ViewName = name ?? throw new ArgumentNullException(nameof(name));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    public string ViewName { get; }

    public IView View
    {
        get
        {
            EnsureInitialized();
            return _view!;
        }
    }

    public INavigationAware ViewModel
    {
        get
        {
            EnsureInitialized();
            return _viewModel!;
        }
    }

    private void EnsureInitialized()
    {
        _view ??= _serviceProvider!.GetRequiredKeyedService<IView>(ViewName);

        _viewModel ??= _serviceProvider!.GetRequiredKeyedService<INavigationAware>(ViewName);

        if (_view.DataContext != _viewModel)
            _view.DataContext = _viewModel;
    }
}
