using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    public MainWindowViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }
    [Reactive]
    private bool _isSplitViewPaneOpen = false;

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigate("MainRegion", viewName, parameters);
    }

    [ReactiveCommand]
    private void AsyncNavigateWithoutWait(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        _ = _regionManager.RequestNavigate("MainRegion", viewName, parameters);
    }

    [ReactiveCommand]
    private async Task AsyncDelayNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        parameters ??= new NavigationParameters();
        parameters!.Add("delay", TimeSpan.FromSeconds(1));
        await _regionManager.RequestNavigate("MainRegion", viewName, parameters);
    }
}
