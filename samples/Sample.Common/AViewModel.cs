using AsyncNavigation.Abstractions;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class AViewModel : ViewModelBase
{
    private static int _instanceCounter = 0;

    private readonly IRegionManager _regionManager;
    public AViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
        InstanceNumber = Interlocked.Increment(ref _instanceCounter);
    }
    public int InstanceNumber { get; }
    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = CommonHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigate("ItemsRegion", viewName, parameters);
    }
}
