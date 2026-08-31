using AsyncNavigation.Abstractions;
using CommunityToolkit.Mvvm.Input;

namespace Sample.Common;

public partial class ListBoxRegionViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    public ListBoxRegionViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }
    [RelayCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigateAsync("CustomListBoxRegion", viewName, parameters);
    }

    [RelayCommand]
    private async Task GoForward()
    {
        await _regionManager.GoForwardAsync("CustomListBoxRegion");
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await _regionManager.GoBackAsync("CustomListBoxRegion");
    }
}
