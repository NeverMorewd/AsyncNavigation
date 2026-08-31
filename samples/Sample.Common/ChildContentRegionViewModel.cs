using AsyncNavigation.Abstractions;
using CommunityToolkit.Mvvm.Input;

namespace Sample.Common;

public partial class ChildContentRegionViewModel : InstanceCounterViewModel<ChildContentRegionViewModel>
{
    private readonly IRegionManager _regionManager;
    public ChildContentRegionViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    [RelayCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigateAsync("ChildContentRegion", viewName, parameters);
    }
    [RelayCommand]
    private Task UnloadView(string param)
    {
        return RequestUnloadAsync(CancellationToken.None);
    }
}
