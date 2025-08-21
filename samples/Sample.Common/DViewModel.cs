using AsyncNavigation.Abstractions;
using AsyncNavigation.Core;
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
        parameters ??= new NavigationParameters();
        parameters!.Add("delay", TimeSpan.FromSeconds(1));
        await _regionManager.RequestNavigate("TabRegion", viewName, parameters);
    }
}
