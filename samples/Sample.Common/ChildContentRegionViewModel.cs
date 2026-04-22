using AsyncNavigation.Abstractions;
using ReactiveUI.SourceGenerators;

namespace Sample.Common;

public partial class ChildContentRegionViewModel : InstanceCounterViewModel<ChildContentRegionViewModel>
{
    private readonly IRegionManager _regionManager;
    public ChildContentRegionViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = SampleHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigateAsync("ChildContentRegion", viewName, parameters);
    }
    [ReactiveCommand]
    private Task UnloadView(string param)
    {
        return RequestUnloadAsync(CancellationToken.None);
    }
}
