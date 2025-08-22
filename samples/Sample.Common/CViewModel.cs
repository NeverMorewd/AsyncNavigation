using AsyncNavigation.Abstractions;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class CViewModel : InstanceCounterViewModel<CViewModel>
{
    private readonly IRegionManager _regionManager;
    public CViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigate("ChildContentRegion", viewName, parameters);
    }
}
