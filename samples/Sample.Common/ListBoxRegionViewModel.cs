using AsyncNavigation.Abstractions;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class ListBoxRegionViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    public ListBoxRegionViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }
    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigate("CustomListBoxRegion", viewName, parameters);
    }

    [ReactiveCommand]
    private async Task GoForward()
    {
        await _regionManager.GoForward("CustomListBoxRegion");
    }

    [ReactiveCommand]
    private async Task GoBack()
    {
        await _regionManager.GoBack("CustomListBoxRegion");
    }
}
