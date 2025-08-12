using AsyncNavigation.Abstractions;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class CViewModel : ViewModelBase
{
    private readonly IRegionManager _regionManager;
    public CViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var result = await _regionManager.RequestNavigate("ChildContentRegion", param);
    }
}
