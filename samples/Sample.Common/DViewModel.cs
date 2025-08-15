using AsyncNavigation.Abstractions;
using ReactiveUI.SourceGenerators;
namespace Sample.Common;

public partial class DViewModel:ViewModelBase
{
    private readonly IRegionManager _regionManager;
    public  DViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) =  CommonHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigate("TabRegion", viewName, parameters);
    }
}
