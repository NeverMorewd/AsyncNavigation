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
    [Reactive]
    private bool _isDialog = false;

    public int InstanceNumber { get; }

    [ReactiveCommand]
    private async Task AsyncNavigate(string param)
    {
        var (viewName, parameters) = CommonHelper.ParseNavigationParam(param);
        await _regionManager.RequestNavigate("ItemsRegion", viewName, parameters);
    }

    [ReactiveCommand]
    private Task UnloadView(string param)
    {
        //return _regionManager.("ItemsRegion", viewName, parameters);
        return RequestUnload();
    }

    [ReactiveCommand]
    private Task CloseDialog(string param)
    {
        return Task.CompletedTask;
    }
}
