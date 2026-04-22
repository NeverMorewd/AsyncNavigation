using AsyncNavigation;
using AsyncNavigation.Abstractions;
using ReactiveUI.SourceGenerators;
namespace Sample.Common;

public partial class TabRegionViewModel : InstanceCounterViewModel<TabRegionViewModel>
{
    private readonly IRegionManager _regionManager;
    public TabRegionViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigateAsync("TabRegion", viewName, parameters);
    }
    [ReactiveCommand]
    private void AsyncNavigateAndForget(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        _ = _regionManager.RequestNavigateAsync("TabRegion", viewName, parameters);
    }

    [ReactiveCommand]
    private async Task GoForward()
    {
        await _regionManager.GoForwardAsync("TabRegion");
    }

    [ReactiveCommand]
    private async Task GoBack()
    {
        await _regionManager.GoBackAsync("TabRegion");
    }
    [ReactiveCommand]
    private async Task RequestUnloadView(NavigationContext navigationContext)
    {
        if (navigationContext.TryResolveNavigationAware(out var aware))
        {
            if (aware is ViewModelBase viewModel)
            {
                await viewModel.RequestUnloadAsync(CancellationToken.None);
            }
        }
    }
}
